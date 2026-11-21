using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Case;
using Fartak.Domain.Catalog;
using Fartak.Domain.Common;
using Fartak.Domain.Framework;
using Fartak.Domain.PlanVersion.Events;
using Fartak.Domain.Unit;
using Fartak.Domain.User.Fartak.Domain.User;

namespace Fartak.Domain.PlanVersion
{
    // نسخه برنامه کشت (جدول plan_versions) + سه فرزند: فازها (plan_phases)،
    // خطوط (plan_lines) و تأییدهای دوگانه (plan_approvals) — تجمع «برنامهٔ نسخه‌دار».
    //
    // قاعده فقط‌خواندنی بودن نسخه‌ها (کامنت اسکیما): محتوای برنامه فقط در وضعیت draft
    // ویرایش می‌شود؛ ویرایش پس از ارسال یعنی ساخت نسخهٔ جدید (V1.0 → V1.1).
    // استثنا: میدان‌های اجرای خطوط (actual//executed/وضعیت خط) که روی نسخهٔ approved
    // ثبت می‌شوند — اینها دادهٔ اجراست، نه محتوای برنامه.
    //
    // چرخه حیات (lookup: plan_status):
    //   draft → pending_approval (SubmitForApproval؛ دو ردیف تأیید در انتظار ساخته می‌شود)
    //   pending_approval → draft (Withdraw؛ فقط اگر هنوز هیچ تصمیمی ثبت نشده باشد)
    //   pending_approval → rejected (هر نقشی رد کند — گذار خودکار)
    //   pending_approval → approved (دو تأیید کامل شد — گذار خودکار طبق کامنت اسکیما)
    //   approved → superseded (MarkSuperseded؛ هنگام تأیید نسخهٔ جدیدتر، توسط Application)
    //
    // total_estimated_cost_rial که در اسکیما با تریگر به‌روز می‌شود، اینجا invariant خود
    // تجمع است و پس از هر تغییر خط بازمحاسبه می‌شود؛ تریگر DB فقط پشتیبان است.
    //
    // یکتایی (case_id, version_no) بین نسخه‌های یک پرونده، بین چند تجمع است → در Application
    // با کنترل تکرار تضمین می‌شود (الگوی کد رهگیری). plot_snapshot یک jsonb آزاد است و
    // اعتبارسنجی ساختارش در لایه Application انجام می‌شود (مثل Meta).
    public class PlanVersion : AggregateRoot<PlanVersionId>
    {
        public const int MaxTitleLength = 150;          // varchar(150) عنوان فاز/خط
        public const int MaxSummaryLength = 4000;       // summary از نوع text
        public const int MaxExpertNoteLength = 2000;    // expert_note از نوع text
        public const int MaxApprovalCommentLength = 1000;
        public const int MaxSortOrder = 32767;          // smallint در اسکیما
        public const int QuantityScale = 3;             // numeric(12,3)
        public const decimal MaxQuantity = 999_999_999.999m;
        public const int ExpectedValueScale = 2;        // numeric(6,2) و numeric(14,2)
        public const decimal MaxExpectedYieldTonPerHa = 9999.99m;
        public const decimal MaxExpectedTotalKg = 999_999_999_999.99m;

        public CaseId CaseId { get; private set; }                 // پروندهٔ صاحب برنامه (تغییرناپذیر)
        public int VersionNo { get; private set; }                 // شماره نسخه (UNIQUE با case در Application)
        public LookupId StatusId { get; private set; }             // lookup: plan_status
        public string CurrentStatusCode { get; private set; } = string.Empty; // کد متنی جاری وضعیت — هم‌زمان با StatusId به‌روز می‌شود

        public UserId? PreparedBy { get; private set; }            // تدوین‌کننده (کارشناس) — تغییرناپذیر
        public DateTime PreparedAt { get; private set; }           // تغییرناپذیر
        public string? PlotSnapshot { get; private set; }          // jsonb فریز ویژگی قطعه/آب هنگام تدوین
        public long TotalEstimatedCostRial { get; private set; }   // جمع هزینه برآوردی خطوط (بازمحاسبه در دامنه)
        public decimal? ExpectedYieldMinTonPerHa { get; private set; }  // numeric(6,2)
        public decimal? ExpectedTotalKg { get; private set; }           // numeric(14,2)
        public DateOnly? HarvestWindowFrom { get; private set; }
        public DateOnly? HarvestWindowTo { get; private set; }
        public string? Summary { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        private readonly List<PlanPhase> _phases = new();
        public IReadOnlyCollection<PlanPhase> Phases => _phases.AsReadOnly();

        private readonly List<PlanLine> _lines = new();
        public IReadOnlyCollection<PlanLine> Lines => _lines.AsReadOnly();

        private readonly List<PlanApproval> _approvals = new();
        public IReadOnlyCollection<PlanApproval> Approvals => _approvals.AsReadOnly();

        private PlanVersion() { } // برای EF Core

        private PlanVersion(
      PlanVersionId id,
      CaseId caseId,
      int versionNo,
      LookupId statusId,
      string statusCode)
        {
            Id = id;
            CaseId = caseId;
            VersionNo = versionNo;
            StatusId = statusId;
            CurrentStatusCode = statusCode;
            PreparedAt = DateTime.UtcNow;
            CreatedAt = PreparedAt;
            UpdatedAt = CreatedAt;
        }

        // ساخت نسخهٔ جدید — همیشه پیش‌نویس خالی؛ فازها و خطوط بعداً در حالت draft اضافه می‌شوند
        public static Result<PlanVersion> Create(
            PlanVersionId id,
            CaseId caseId,
            int versionNo,
            LookupId initialStatusId,
            string initialStatusCode,
            UserId? preparedBy = null,
            string? plotSnapshot = null,
            decimal? expectedYieldMinTonPerHa = null,
            decimal? expectedTotalKg = null,
            DateOnly? harvestWindowFrom = null,
            DateOnly? harvestWindowTo = null,
            string? summary = null)
        {
            if (id is null || caseId is null || initialStatusId is null || initialStatusCode is null)
                return Result.Failure<PlanVersion>(PlanVersionErrors.NullReferenceError());

            if (versionNo <= 0)
                return Result.Failure<PlanVersion>(PlanVersionErrors.InvalidVersionNo(versionNo));

            var normalizedStatus = initialStatusCode.Trim();
            
            if (normalizedStatus != WellKnownPlanStatuses.Draft)
                return Result.Failure<PlanVersion>(PlanVersionErrors.InvalidInitialStatus(normalizedStatus));

            var harvestValidation = ValidateHarvestWindow(harvestWindowFrom, harvestWindowTo);
            if (harvestValidation.IsFailure)
                return Result.Failure<PlanVersion>(harvestValidation.Error);

            var yieldValidation = ValidateExpectedValues(expectedYieldMinTonPerHa, expectedTotalKg);
            if (yieldValidation.IsFailure)
                return Result.Failure<PlanVersion>(yieldValidation.Error);

            var normalizedSummary = string.IsNullOrWhiteSpace(summary) ? null : summary.Trim();
            if (normalizedSummary is not null && normalizedSummary.Length > MaxSummaryLength)
                return Result.Failure<PlanVersion>(PlanVersionErrors.SummaryTooLong(normalizedSummary.Length));

            var version = new PlanVersion(id, caseId, versionNo, initialStatusId, normalizedStatus);
            version.PreparedBy = preparedBy;
            version.PlotSnapshot = string.IsNullOrWhiteSpace(plotSnapshot) ? null : plotSnapshot.Trim();
            version.ExpectedYieldMinTonPerHa = expectedYieldMinTonPerHa;
            version.ExpectedTotalKg = expectedTotalKg;
            version.HarvestWindowFrom = harvestWindowFrom;
            version.HarvestWindowTo = harvestWindowTo;
            version.Summary = normalizedSummary;

            version.AddDomainEvent(new PlanVersionCreatedEvent(id.Value, caseId.Value, versionNo));
            return Result<PlanVersion>.Success(version);
        }

        // ویرایش اطلاعات سراسری نسخه (پنجره برداشت، عملکرد موردانتظار، خلاصه، snapshot قطعه)
        // — فقط در پیش‌نویس؛ فازها و خطوط با متدهای خودشان مدیریت می‌شوند
        public Result UpdatePlanInfo(
            string? plotSnapshot,
            decimal? expectedYieldMinTonPerHa,
            decimal? expectedTotalKg,
            DateOnly? harvestWindowFrom,
            DateOnly? harvestWindowTo,
            string? summary)
        {
            var draftCheck = EnsureDraft();
            if (draftCheck.IsFailure)
                return draftCheck;

            var harvestValidation = ValidateHarvestWindow(harvestWindowFrom, harvestWindowTo);
            if (harvestValidation.IsFailure)
                return harvestValidation;

            var yieldValidation = ValidateExpectedValues(expectedYieldMinTonPerHa, expectedTotalKg);
            if (yieldValidation.IsFailure)
                return yieldValidation;

            var normalizedSnapshot = string.IsNullOrWhiteSpace(plotSnapshot) ? null : plotSnapshot.Trim();
            var normalizedSummary = string.IsNullOrWhiteSpace(summary) ? null : summary.Trim();
            if (normalizedSummary is not null && normalizedSummary.Length > MaxSummaryLength)
                return Result.Failure(PlanVersionErrors.SummaryTooLong(normalizedSummary.Length));

            if (normalizedSnapshot == PlotSnapshot
                && expectedYieldMinTonPerHa == ExpectedYieldMinTonPerHa
                && expectedTotalKg == ExpectedTotalKg
                && harvestWindowFrom == HarvestWindowFrom
                && harvestWindowTo == HarvestWindowTo
                && normalizedSummary == Summary)
                return Result.Failure(PlanVersionErrors.PlanInfoUnchanged());

            PlotSnapshot = normalizedSnapshot;
            ExpectedYieldMinTonPerHa = expectedYieldMinTonPerHa;
            ExpectedTotalKg = expectedTotalKg;
            HarvestWindowFrom = harvestWindowFrom;
            HarvestWindowTo = harvestWindowTo;
            Summary = normalizedSummary;
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new PlanVersionInfoUpdatedEvent(Id.Value));
            return Result.Success();
        }

        // ── فازها ────────────────────────────────────────────────────────────

        public Result AddPhase(PlanPhaseId planPhaseId, string title, int sortOrder)
        {
            var draftCheck = EnsureDraft();
            if (draftCheck.IsFailure)
                return draftCheck;

            if (planPhaseId is null)
                return Result.Failure(PlanVersionErrors.NullReferenceError());

            var titleValidation = ValidateTitle(title);
            if (titleValidation.IsFailure)
                return titleValidation;

            if (IsInvalidSortOrder(sortOrder))
                return Result.Failure(PlanVersionErrors.SortOrderOutOfRange());

            var phase = PlanPhase.Create(planPhaseId, title.Trim(), sortOrder);
            _phases.Add(phase);

            AddDomainEvent(new PlanPhaseAddedEvent(
                planPhaseId.Value, Id.Value, phase.Title, sortOrder));
            return Result.Success();
        }
        public Result RenamePhase(PlanPhaseId planPhaseId, string newTitle)
        {
            var draftCheck = EnsureDraft();
            if (draftCheck.IsFailure)
                return draftCheck;

            var phase = FindPhase(planPhaseId);
            if (phase is null)
                return Result.Failure(PlanVersionErrors.PhaseNotFound(planPhaseId.Value));

            var titleValidation = ValidateTitle(newTitle);
            if (titleValidation.IsFailure)
                return titleValidation;

            var normalizedTitle = newTitle.Trim();
            if (normalizedTitle == phase.Title)
                return Result.Failure(PlanVersionErrors.PhaseTitleUnchanged());

            var oldTitle = phase.Title;
            phase.SetTitle(normalizedTitle);
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new PlanPhaseRenamedEvent(
                planPhaseId.Value, Id.Value, oldTitle, phase.Title));
            return Result.Success();
        }
        public Result ChangePhaseSortOrder(PlanPhaseId planPhaseId, int newSortOrder)
        {
            var draftCheck = EnsureDraft();
            if (draftCheck.IsFailure)
                return draftCheck;

            var phase = FindPhase(planPhaseId);
            if (phase is null)
                return Result.Failure(PlanVersionErrors.PhaseNotFound(planPhaseId.Value));

            if (IsInvalidSortOrder(newSortOrder))
                return Result.Failure(PlanVersionErrors.SortOrderOutOfRange());

            if (newSortOrder == phase.SortOrder)
                return Result.Failure(PlanVersionErrors.PhaseSortOrderUnchanged());

            var oldSortOrder = phase.SortOrder;
            phase.SetSortOrder(newSortOrder);
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new PlanPhaseSortOrderChangedEvent(
                planPhaseId.Value, Id.Value, oldSortOrder, newSortOrder));
            return Result.Success();
        }

        // حذف فاز — اگر خطی به آن متصل باشد ممنوع است تا خط یتیم نشود
        public Result RemovePhase(PlanPhaseId planPhaseId)
        {
            var draftCheck = EnsureDraft();
            if (draftCheck.IsFailure)
                return draftCheck;

            var phase = FindPhase(planPhaseId);
            if (phase is null)
                return Result.Failure(PlanVersionErrors.PhaseNotFound(planPhaseId.Value));

            var linkedLineCount = _lines.Count(l => l.PhaseId?.Value == planPhaseId.Value);
            if (linkedLineCount > 0)
                return Result.Failure(PlanVersionErrors.PhaseHasLines(planPhaseId.Value, linkedLineCount));

            _phases.Remove(phase);
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new PlanPhaseRemovedEvent(planPhaseId.Value, Id.Value));
            return Result.Success();
        }

        // ── خطوط برنامه ──────────────────────────────────────────────────────

        // افزودن خط (قلم/اقدام) — فقط در پیش‌نویس؛ خط با وضعیت آغازین «برنامه‌ریزی‌شده» ساخته می‌شود
        public Result AddLine(
            PlanLineId planLineId,
            LookupId lineStatusId,
            string lineStatusCode,
            string lineTypeCode,
            string title,
            decimal quantity,
            LookupId qtyBasisId,
            UnitId unitId,
            LookupId responsibleId,
            long estimatedCostRial,
            PlanPhaseId? phaseId = null,
            CatalogItemId? catalogItemId = null,
            LookupId? supplyMethodId = null,
            DateOnly? plannedDate = null,
            DateOnly? plannedEndDate = null,
            string? expertNote = null,
            int sortOrder = 0)
        {
            var draftCheck = EnsureDraft();
            if (draftCheck.IsFailure)
                return draftCheck;

            if (planLineId is null || lineStatusId is null || lineStatusCode is null
                || qtyBasisId is null || unitId is null || responsibleId is null)
                return Result.Failure(PlanVersionErrors.NullReferenceError());

            var planningValidation = ValidateLinePlanning(
     lineTypeCode, catalogItemId, title, quantity, qtyBasisId, unitId, supplyMethodId,
     responsibleId, plannedDate, plannedEndDate, estimatedCostRial, expertNote, sortOrder, phaseId);
            if (planningValidation.IsFailure)
                return planningValidation;

            var normalizedLineStatus = lineStatusCode.Trim();
            if (!IsKnownLineStatusCode(normalizedLineStatus))
                return Result.Failure(PlanVersionErrors.UnknownLineStatusCode(normalizedLineStatus));

            if (normalizedLineStatus != WellKnownLineStatuses.Planned)
                return Result.Failure(PlanVersionErrors.InvalidLineInitialStatus(normalizedLineStatus));

            var line = PlanLine.Create(
                planLineId, phaseId, lineTypeCode.Trim(), catalogItemId,
                title.Trim(), quantity, qtyBasisId, unitId, supplyMethodId, responsibleId,
                plannedDate, plannedEndDate, estimatedCostRial,
                string.IsNullOrWhiteSpace(expertNote) ? null : expertNote.Trim(),
                sortOrder, lineStatusId);
            _lines.Add(line);
            RecalculateTotalEstimatedCost();
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new PlanLineAddedEvent(
                planLineId.Value, Id.Value, line.LineType, estimatedCostRial));
            return Result.Success();
        }

        // ویرایش میدان‌های برنامه‌ریزی خط — فقط در پیش‌نویس؛
        // line_type تغییر نمی‌کند (تغییر نوع = حذف و ساخت مجدد) و میدان‌های اجرا دست‌نخورده‌اند
        public Result UpdateLine(
            PlanLineId planLineId,
            PlanPhaseId? phaseId,
            CatalogItemId? catalogItemId,
            string title,
            decimal quantity,
            LookupId qtyBasisId,
            UnitId unitId,
            LookupId? supplyMethodId,
            LookupId responsibleId,
            DateOnly? plannedDate,
            DateOnly? plannedEndDate,
            long estimatedCostRial,
            string? expertNote,
            int sortOrder)
        {
            var draftCheck = EnsureDraft();
            if (draftCheck.IsFailure)
                return draftCheck;

            var line = FindLine(planLineId);
            if (line is null)
                return Result.Failure(PlanVersionErrors.LineNotFound(planLineId.Value));

            if (qtyBasisId is null || unitId is null || responsibleId is null)
                return Result.Failure(PlanVersionErrors.NullReferenceError());

            var planningValidation = ValidateLinePlanning(
    line.LineType, catalogItemId, title, quantity, qtyBasisId, unitId, supplyMethodId,
    responsibleId, plannedDate, plannedEndDate, estimatedCostRial, expertNote, sortOrder, phaseId);
            if (planningValidation.IsFailure)
                return planningValidation;

            var normalizedNote = string.IsNullOrWhiteSpace(expertNote) ? null : expertNote.Trim();
            if (phaseId?.Value == line.PhaseId?.Value
                && catalogItemId?.Value == line.CatalogItemId?.Value
                && title.Trim() == line.Title
                && quantity == line.Quantity
                && qtyBasisId.Value == line.QtyBasisId.Value
                && unitId.Value == line.UnitId.Value
                && supplyMethodId?.Value == line.SupplyMethodId?.Value
                && responsibleId.Value == line.ResponsibleId.Value
                && plannedDate == line.PlannedDate
                && plannedEndDate == line.PlannedEndDate
                && estimatedCostRial == line.EstimatedCostRial
                && normalizedNote == line.ExpertNote
                && sortOrder == line.SortOrder)
                return Result.Failure(PlanVersionErrors.LineUnchanged());

            line.UpdatePlanning(
                phaseId, catalogItemId, title.Trim(), quantity, qtyBasisId, unitId,
                supplyMethodId, responsibleId, plannedDate, plannedEndDate,
                estimatedCostRial, normalizedNote, sortOrder);
            RecalculateTotalEstimatedCost();
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new PlanLineUpdatedEvent(
                planLineId.Value, Id.Value, line.EstimatedCostRial));
            return Result.Success();
        }

        public Result RemoveLine(PlanLineId planLineId)
        {
            var draftCheck = EnsureDraft();
            if (draftCheck.IsFailure)
                return draftCheck;

            var line = FindLine(planLineId);
            if (line is null)
                return Result.Failure(PlanVersionErrors.LineNotFound(planLineId.Value));

            _lines.Remove(line);
            RecalculateTotalEstimatedCost();
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new PlanLineRemovedEvent(planLineId.Value, Id.Value));
            return Result.Success();
        }

        // ── چرخه تأیید دوگانه ────────────────────────────────────────────────

        // ارسال برای تأیید: نسخه منجمد می‌شود و دو ردیف تأییدِ «در انتظار» (کارشناس + کشاورز)
        // ساخته می‌شوند؛ برنامهٔ بدون خط قابل ارسال نیست
        public Result SubmitForApproval(
            LookupId pendingStatusId,
            string pendingStatusCode,
            PlanApprovalId expertApprovalId,
            PlanApprovalId farmerApprovalId)
        {
            var draftCheck = EnsureDraft();
            if (draftCheck.IsFailure)
                return draftCheck;

            if (pendingStatusId is null || expertApprovalId is null || farmerApprovalId is null)
                return Result.Failure(PlanVersionErrors.NullReferenceError());

            var normalizedTarget = pendingStatusCode?.Trim();
            if (!IsKnownPlanStatusCode(normalizedTarget))
                return Result.Failure(PlanVersionErrors.UnknownPlanStatusCode(normalizedTarget));

            if (normalizedTarget != WellKnownPlanStatuses.PendingApproval)
                return Result.Failure(PlanVersionErrors.InvalidSubmitTargetStatus(normalizedTarget));

            if (_lines.Count == 0)
                return Result.Failure(PlanVersionErrors.EmptyPlanCannotSubmit());

            _approvals.Add(PlanApproval.Create(expertApprovalId, WellKnownApproverRoles.Expert));
            _approvals.Add(PlanApproval.Create(farmerApprovalId, WellKnownApproverRoles.Farmer));
            StatusId = pendingStatusId;
            CurrentStatusCode = normalizedTarget;
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new PlanVersionSubmittedForApprovalEvent(
                Id.Value, CaseId.Value, expertApprovalId.Value, farmerApprovalId.Value));
            return Result.Success();
        }

        // بازگشت به پیش‌نویس — فقط وقتی هنوز هیچ تصمیمی ثبت نشده باشد؛
        // ردیف‌های در انتظار پاک می‌شوند و در ارسال بعدی ردیف تازه ساخته می‌شود
        public Result Withdraw(LookupId draftStatusId, string draftStatusCode)
        {
            var pendingCheck = EnsurePendingApproval();
            if (pendingCheck.IsFailure)
                return pendingCheck;

            if (draftStatusId is null)
                return Result.Failure(PlanVersionErrors.NullReferenceError());

            var normalizedTarget = draftStatusCode?.Trim();
            if (!IsKnownPlanStatusCode(normalizedTarget))
                return Result.Failure(PlanVersionErrors.UnknownPlanStatusCode(normalizedTarget));

            if (normalizedTarget != WellKnownPlanStatuses.Draft)
                return Result.Failure(PlanVersionErrors.InvalidWithdrawTargetStatus(normalizedTarget));

            if (_approvals.Any(a => a.IsDecided))
                return Result.Failure(PlanVersionErrors.WithdrawBlockedByDecisions());

            _approvals.Clear();
            StatusId = draftStatusId;
            CurrentStatusCode = normalizedTarget;
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new PlanVersionWithdrawnEvent(Id.Value));
            return Result.Success();
        }

        // ثبت تصمیم یکی از نقش‌ها؛ گذار خودکار نسخه:
        //   هر رد → rejected | دو تأیید کامل → approved (طبق کامنت اسکیما)
        public Result RecordApprovalDecision(
            PlanApprovalId approvalId,
            LookupId decisionId,
            string decisionCode,
            DateTime decidedAt,
            UserId? approvedBy,
            string? comment,
            LookupId approvedStatusId,
            string approvedStatusCode,
            LookupId rejectedStatusId,
            string rejectedStatusCode)
        {
            var pendingCheck = EnsurePendingApproval();
            if (pendingCheck.IsFailure)
                return pendingCheck;

            if (approvalId is null || decisionId is null
                || approvedStatusId is null || rejectedStatusId is null)
                return Result.Failure(PlanVersionErrors.NullReferenceError());

            var normalizedDecision = decisionCode?.Trim();
            if (!IsKnownDecisionCode(normalizedDecision))
                return Result.Failure(PlanVersionErrors.UnknownDecisionCode(normalizedDecision));

            var normalizedApproved = approvedStatusCode?.Trim();
            var normalizedRejected = rejectedStatusCode?.Trim();
            if (!IsKnownPlanStatusCode(normalizedApproved) || !IsKnownPlanStatusCode(normalizedRejected))
                return Result.Failure(PlanVersionErrors.UnknownPlanStatusCode(normalizedApproved ?? normalizedRejected));

            if (normalizedApproved != WellKnownPlanStatuses.Approved
                || normalizedRejected != WellKnownPlanStatuses.Rejected)
                return Result.Failure(PlanVersionErrors.InvalidDecisionTargetStatuses(normalizedApproved, normalizedRejected));

            var approval = FindApproval(approvalId);
            if (approval is null)
                return Result.Failure(PlanVersionErrors.ApprovalNotFound(approvalId.Value));

            if (approval.IsDecided)
                return Result.Failure(PlanVersionErrors.ApprovalDecisionAlreadyRecorded(approvalId.Value));

            var normalizedComment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
            if (normalizedComment is not null && normalizedComment.Length > MaxApprovalCommentLength)
                return Result.Failure(PlanVersionErrors.ApprovalCommentTooLong(normalizedComment.Length));

            approval.RecordDecision(decisionId, approvedBy, decidedAt, normalizedComment);
            AddDomainEvent(new PlanApprovalRecordedEvent(
                approvalId.Value, Id.Value, approval.ApproverRole, normalizedDecision));

            if (normalizedDecision == WellKnownDecisions.Rejected)
            {
                StatusId = rejectedStatusId;
                CurrentStatusCode = normalizedRejected;
                UpdatedAt = DateTime.UtcNow;
                AddDomainEvent(new PlanVersionRejectedEvent(Id.Value, CaseId.Value));
                return Result.Success();
            }

            if (_approvals.All(a => a.IsDecided))
            {
                StatusId = approvedStatusId;
                CurrentStatusCode = normalizedApproved;
                UpdatedAt = DateTime.UtcNow;
                AddDomainEvent(new PlanVersionApprovedEvent(Id.Value, CaseId.Value));
            }

            return Result.Success();
        }

        // جایگزین‌شدن نسخه توسط نسخهٔ جدیدتر — پس از تأیید نسخهٔ جدید، Application این را صدا می‌زند
        public Result MarkSuperseded(LookupId supersededStatusId, string supersededStatusCode)
        {
            var approvedCheck = EnsureApproved();
            if (approvedCheck.IsFailure)
                return approvedCheck;

            if (supersededStatusId is null)
                return Result.Failure(PlanVersionErrors.NullReferenceError());

            var normalizedTarget = supersededStatusCode?.Trim();
            if (!IsKnownPlanStatusCode(normalizedTarget))
                return Result.Failure(PlanVersionErrors.UnknownPlanStatusCode(normalizedTarget));

            if (normalizedTarget != WellKnownPlanStatuses.Superseded)
                return Result.Failure(PlanVersionErrors.InvalidSupersededTargetStatus(normalizedTarget));

            StatusId = supersededStatusId;
            CurrentStatusCode = normalizedTarget;
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new PlanVersionSupersededEvent(Id.Value, CaseId.Value));
            return Result.Success();
        }

        // ── اجرای خطوط (فقط روی نسخهٔ تأییدشده) ──────────────────────────────

        // ثبت اجرای واقعی خط؛ خط با وضعیت «انجام‌شده» بسته می‌شود — مبنای v_case_progress
        public Result RecordLineExecution(
            PlanLineId planLineId,
            string currentLineStatusCode,
            LookupId doneStatusId,
            string doneStatusCode,
            decimal? actualQty,
            long? actualCostRial,
            DateTime? executedAt = null,
            UserId? executedBy = null)
        {
            var approvedCheck = EnsureApproved();
            if (approvedCheck.IsFailure)
                return approvedCheck;

            if (doneStatusId is null)
                return Result.Failure(PlanVersionErrors.NullReferenceError());

            var line = FindLine(planLineId);
            if (line is null)
                return Result.Failure(PlanVersionErrors.LineNotFound(planLineId.Value));

            var normalizedCurrent = currentLineStatusCode?.Trim();
            var lineStatusCheck = EnsureKnownLineStatus(normalizedCurrent);
            if (lineStatusCheck.IsFailure)
                return lineStatusCheck;

            var normalizedDone = doneStatusCode?.Trim();
            if (!IsKnownLineStatusCode(normalizedDone))
                return Result.Failure(PlanVersionErrors.UnknownLineStatusCode(normalizedDone));

            if (normalizedDone != WellKnownLineStatuses.Done)
                return Result.Failure(PlanVersionErrors.InvalidExecutionTargetStatus(normalizedDone));

            if (normalizedCurrent == normalizedDone)
                return Result.Failure(PlanVersionErrors.LineStatusUnchanged(planLineId.Value));

            if (!AllowedLineTransitions[normalizedCurrent].Contains(normalizedDone))
                return Result.Failure(PlanVersionErrors.InvalidLineStatusTransition(normalizedCurrent, normalizedDone));

            var qtyCheck = ValidateQuantity(actualQty, required: false);
            if (qtyCheck.IsFailure)
                return qtyCheck;

            if (actualCostRial is < 0)
                return Result.Failure(PlanVersionErrors.CostCannotBeNegative());

            line.RecordExecution(
                doneStatusId, actualQty, actualCostRial,
                executedAt ?? DateTime.UtcNow, executedBy);
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new PlanLineExecutionRecordedEvent(
                planLineId.Value, Id.Value, line.ExecutedAt!.Value));
            return Result.Success();
        }

        // گذار وضعیت خط بدون ثبت اجرا (شروع کار، تأخیر، حذف، لغو) — فقط روی نسخهٔ تأییدشده
        public Result ChangeLineStatus(
            PlanLineId planLineId,
            string currentLineStatusCode,
            LookupId newStatusId,
            string newStatusCode)
        {
            var approvedCheck = EnsureApproved();
            if (approvedCheck.IsFailure)
                return approvedCheck;

            if (newStatusId is null)
                return Result.Failure(PlanVersionErrors.NullReferenceError());

            var line = FindLine(planLineId);
            if (line is null)
                return Result.Failure(PlanVersionErrors.LineNotFound(planLineId.Value));

            var normalizedCurrent = currentLineStatusCode?.Trim();
            var lineStatusCheck = EnsureKnownLineStatus(normalizedCurrent);
            if (lineStatusCheck.IsFailure)
                return lineStatusCheck;

            var normalizedNew = newStatusCode?.Trim();
            if (!IsKnownLineStatusCode(normalizedNew))
                return Result.Failure(PlanVersionErrors.UnknownLineStatusCode(normalizedNew));

            if (normalizedNew == normalizedCurrent)
                return Result.Failure(PlanVersionErrors.LineStatusUnchanged(planLineId.Value));

            if (!AllowedLineTransitions[normalizedCurrent].Contains(normalizedNew))
                return Result.Failure(PlanVersionErrors.InvalidLineStatusTransition(normalizedCurrent, normalizedNew));

            line.SetStatus(newStatusId);
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new PlanLineStatusChangedEvent(
                planLineId.Value, Id.Value, normalizedNew));
            return Result.Success();
        }

        // ── کدهای معروف دامنه‌ها (مطابق سید اسکیما) ──────────────────────────

        public static class WellKnownPlanStatuses
        {
            public const string Draft = "draft";                       // پیش‌نویس — تنها وضعیت ویرایش‌پذیر
            public const string PendingApproval = "pending_approval";  // در انتظار تأیید دوگانه
            public const string Approved = "approved";                 // تأییدشده
            public const string Rejected = "rejected";                 // ردشده — پایانی
            public const string Superseded = "superseded";             // جایگزین‌شده توسط نسخهٔ جدیدتر
        }

        public static class WellKnownLineStatuses
        {
            public const string Planned = "planned";        // برنامه‌ریزی‌شده — وضعیت آغازین خط
            public const string InProgress = "in_progress"; // در حال اجرا
            public const string Done = "done";              // انجام‌شده — با ثبت اجرا بسته می‌شود
            public const string Delayed = "delayed";        // با تأخیر
            public const string Skipped = "skipped";        // حذف‌شده
            public const string Canceled = "canceled";      // لغوشده (املای مرجع ERD)
        }

        public static class WellKnownLineTypes
        {
            public const string Item = "item";     // قلم تأمینی — متصل به کاتالوگ، شرکت‌پذیر در سفارش تجمیعی
            public const string Action = "action"; // اقدام اجرایی — عنوان آزاد، بدون کاتالوگ
        }

        public static class WellKnownApproverRoles
        {
            public const string Expert = "expert"; // کارشناس (منتور)
            public const string Farmer = "farmer"; // کشاورز
        }

        public static class WellKnownDecisions
        {
            public const string Approved = "approved"; // تأیید
            public const string Rejected = "rejected"; // رد
        }

        // گذارهای مجاز وضعیت خط — قاعده دامنه؛ خطوط بسته (done/skipped/canceled) پایانی‌اند
        private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> AllowedLineTransitions =
            new Dictionary<string, IReadOnlyCollection<string>>
            {
                [WellKnownLineStatuses.Planned] = new[]
                {
                    WellKnownLineStatuses.InProgress, WellKnownLineStatuses.Done,
                    WellKnownLineStatuses.Delayed, WellKnownLineStatuses.Skipped,
                    WellKnownLineStatuses.Canceled
                },
                [WellKnownLineStatuses.InProgress] = new[]
                {
                    WellKnownLineStatuses.Done, WellKnownLineStatuses.Delayed,
                    WellKnownLineStatuses.Skipped, WellKnownLineStatuses.Canceled
                },
                [WellKnownLineStatuses.Delayed] = new[]
                {
                    WellKnownLineStatuses.InProgress, WellKnownLineStatuses.Done,
                    WellKnownLineStatuses.Skipped, WellKnownLineStatuses.Canceled
                },
                [WellKnownLineStatuses.Done] = Array.Empty<string>(),
                [WellKnownLineStatuses.Skipped] = Array.Empty<string>(),
                [WellKnownLineStatuses.Canceled] = Array.Empty<string>()
            };

        // ── helperهای خصوصی ──────────────────────────────────────────────────

        private bool IsKnownPlanStatusCode(string? code)
            => code == WellKnownPlanStatuses.Draft
            || code == WellKnownPlanStatuses.PendingApproval
            || code == WellKnownPlanStatuses.Approved
            || code == WellKnownPlanStatuses.Rejected
            || code == WellKnownPlanStatuses.Superseded;

        private static bool IsKnownLineStatusCode(string? code)
            => code == WellKnownLineStatuses.Planned
            || code == WellKnownLineStatuses.InProgress
            || code == WellKnownLineStatuses.Done
            || code == WellKnownLineStatuses.Delayed
            || code == WellKnownLineStatuses.Skipped
            || code == WellKnownLineStatuses.Canceled;

        private static bool IsKnownDecisionCode(string? code)
            => code == WellKnownDecisions.Approved
            || code == WellKnownDecisions.Rejected;

        private static Result EnsureKnownLineStatus(string? code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return Result.Failure(PlanVersionErrors.NullReferenceError());

            if (!IsKnownLineStatusCode(code.Trim()))
                return Result.Failure(PlanVersionErrors.UnknownLineStatusCode(code.Trim()));

            return Result.Success();
        }
       private string ResolveCurrentPlanStatusCode()
   => CurrentStatusCode;
        private Result EnsureDraft()
        {
            var current = ResolveCurrentPlanStatusCode();
            if (!IsKnownPlanStatusCode(current))
                return Result.Failure(PlanVersionErrors.UnknownPlanStatusCode(current));

            if (current != WellKnownPlanStatuses.Draft)
                return Result.Failure(PlanVersionErrors.VersionNotDraft(current));

            return Result.Success();
        }

        private Result EnsurePendingApproval()
        {
            var current = ResolveCurrentPlanStatusCode();
            if (!IsKnownPlanStatusCode(current))
                return Result.Failure(PlanVersionErrors.UnknownPlanStatusCode(current));

            if (current != WellKnownPlanStatuses.PendingApproval)
                return Result.Failure(PlanVersionErrors.VersionNotPendingApproval(current));

            return Result.Success();
        }

        private Result EnsureApproved()
        {
            var current = ResolveCurrentPlanStatusCode();
            if (!IsKnownPlanStatusCode(current))
                return Result.Failure(PlanVersionErrors.UnknownPlanStatusCode(current));

            if (current != WellKnownPlanStatuses.Approved)
                return Result.Failure(PlanVersionErrors.VersionNotApproved(current));

            return Result.Success();
        }

        private PlanPhase FindPhase(PlanPhaseId? phaseId)
            => phaseId is null
                ? null
                : _phases.FirstOrDefault(p => p.Id.Value == phaseId.Value);

        private PlanLine FindLine(PlanLineId? lineId)
            => lineId is null
                ? null
                : _lines.FirstOrDefault(l => l.Id.Value == lineId.Value);

        private PlanApproval FindApproval(PlanApprovalId? approvalId)
            => approvalId is null
                ? null
                : _approvals.FirstOrDefault(a => a.Id.Value == approvalId.Value);

        private void RecalculateTotalEstimatedCost()
            => TotalEstimatedCostRial = _lines.Sum(l => l.EstimatedCostRial);

        private static bool IsInvalidSortOrder(int sortOrder)
            => sortOrder < 0 || sortOrder > MaxSortOrder;

        private static Result<string> ValidateTitle(string? title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return Result.Failure<string>(PlanVersionErrors.InvalidTitle());

            var normalizedTitle = title.Trim();
            if (normalizedTitle.Length > MaxTitleLength)
                return Result.Failure<string>(PlanVersionErrors.TitleTooLong(normalizedTitle.Length));

            return Result<string>.Success(normalizedTitle);
        }

        private static Result ValidateQuantity(decimal? quantity, bool required)
        {
            if (quantity is null)
                return required
                    ? Result.Failure(PlanVersionErrors.InvalidQuantity())
                    : Result.Success();

            if (quantity <= 0)
                return Result.Failure(PlanVersionErrors.InvalidQuantity());

            if (decimal.Round(quantity.Value, QuantityScale) != quantity.Value)
                return Result.Failure(PlanVersionErrors.QuantityTooPrecise());

            if (quantity > MaxQuantity)
                return Result.Failure(PlanVersionErrors.QuantityOutOfRange());

            return Result.Success();
        }

        private static Result ValidateHarvestWindow(DateOnly? from, DateOnly? to)
        {
            if (from is not null && to is not null && to < from)
                return Result.Failure(PlanVersionErrors.InvalidHarvestWindow(
                    from.Value.ToDateTime(TimeOnly.MinValue),
                    to.Value.ToDateTime(TimeOnly.MinValue)));

            return Result.Success();
        }

        private static Result ValidateExpectedValues(decimal? expectedYieldMinTonPerHa, decimal? expectedTotalKg)
        {
            if (expectedYieldMinTonPerHa is not null)
            {
                if (decimal.Round(expectedYieldMinTonPerHa.Value, ExpectedValueScale) != expectedYieldMinTonPerHa.Value
                    || expectedYieldMinTonPerHa <= 0
                    || expectedYieldMinTonPerHa > MaxExpectedYieldTonPerHa)
                    return Result.Failure(PlanVersionErrors.ExpectedYieldInvalid(expectedYieldMinTonPerHa));
            }

            if (expectedTotalKg is not null)
            {
                if (decimal.Round(expectedTotalKg.Value, ExpectedValueScale) != expectedTotalKg.Value
                    || expectedTotalKg <= 0
                    || expectedTotalKg > MaxExpectedTotalKg)
                    return Result.Failure(PlanVersionErrors.ExpectedTotalKgInvalid(expectedTotalKg));
            }

            return Result.Success();
        }

        // قواعد مشترک خط (قلم/اقدام) — بین AddLine و UpdateLine مشترک است
        private Result ValidateLinePlanning(
            string? lineTypeCode,
            CatalogItemId? catalogItemId,
            string? title,
            decimal quantity,
            LookupId? qtyBasisId,
            UnitId? unitId,
            LookupId? supplyMethodId,
            LookupId? responsibleId,
            DateOnly? plannedDate,
            DateOnly? plannedEndDate,
            long estimatedCostRial,
            string? expertNote,
            int sortOrder,
            PlanPhaseId? phaseId)
        {
            var normalizedType = lineTypeCode?.Trim();
            if (normalizedType != WellKnownLineTypes.Item && normalizedType != WellKnownLineTypes.Action)
                return Result.Failure(PlanVersionErrors.UnknownLineType(normalizedType));

            var titleValidation = ValidateTitle(title);
            if (titleValidation.IsFailure)
                return titleValidation;

            var quantityValidation = ValidateQuantity(quantity, required: true);
            if (quantityValidation.IsFailure)
                return quantityValidation;

            if (qtyBasisId is null || unitId is null || responsibleId is null)
                return Result.Failure(PlanVersionErrors.NullReferenceError());

            if (normalizedType == WellKnownLineTypes.Item && catalogItemId is null)
                return Result.Failure(PlanVersionErrors.ItemRequiresCatalogItem());

            if (normalizedType == WellKnownLineTypes.Action && catalogItemId is not null)
                return Result.Failure(PlanVersionErrors.ActionMustNotHaveCatalogItem());

            if (normalizedType == WellKnownLineTypes.Action && supplyMethodId is not null)
                return Result.Failure(PlanVersionErrors.SupplyMethodOnlyForItem());

            if (estimatedCostRial < 0)
                return Result.Failure(PlanVersionErrors.CostCannotBeNegative());

            if (plannedDate is not null && plannedEndDate is not null && plannedEndDate < plannedDate)
                return Result.Failure(PlanVersionErrors.InvalidPlannedDateRange(plannedDate.Value, plannedEndDate.Value));

            var normalizedNote = string.IsNullOrWhiteSpace(expertNote) ? null : expertNote.Trim();
            if (normalizedNote is not null && normalizedNote.Length > MaxExpertNoteLength)
                return Result.Failure(PlanVersionErrors.ExpertNoteTooLong(normalizedNote.Length));

            if (IsInvalidSortOrder(sortOrder))
                return Result.Failure(PlanVersionErrors.SortOrderOutOfRange());

            if (phaseId is not null && FindPhase(phaseId) is null)
                return Result.Failure(PlanVersionErrors.PhaseNotFound(phaseId.Value));

            return Result.Success();
        }

       
    }
}
