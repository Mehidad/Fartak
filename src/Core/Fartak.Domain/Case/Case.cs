using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Catalog;
using Fartak.Domain.Common;
using Fartak.Domain.Framework;
using Fartak.Domain.Plot;
using Fartak.Domain.User.Fartak.Domain.User;

namespace Fartak.Domain.Case
{
    // پرونده — قلب چرخه کار فرتاک: یک قطعه زمین × یک محصول × یک سال زراعی (جدول cases)
    // تاریخچه وضعیت (case_status_logs)، تاریخچه تخصیص (case_assignments)
    // و یادداشت‌ها (case_notes) همگی موجودیت‌های فرزندِ همین Aggregate هستند و
    // فقط از طریق متدهای خود Case ساخته می‌شوند تا ناسازگاری تاریخچه ممکن نباشد.
    // Area همان VO مساحتِ Aggregate Plot است (Snapshot مساحت قطعه هنگام ثبت پرونده).
    public class Case : AggregateRoot<CaseId>
    {
        public TrackingCode TrackingCode { get; private set; }    // FT-1403-02867 (unique)
        public UserId FarmerId { get; private set; }              // کشاورز مالک پرونده
        public PlotId PlotId { get; private set; }                // قطعه زمین
        public CropId CropId { get; private set; }                // محصول
        public VarietyId? VarietyId { get; private set; }         // رقم (اختیاری)
        public SeasonYear SeasonYear { get; private set; }        // سال زراعی جلالی
        public DateOnly? PlantingDate { get; private set; }       // تاریخ کشت (میلادی در DB)
        public Area Area { get; private set; }                    // Snapshot مساحت قطعه هنگام ثبت
        public CaseStatusId CurrentStatusId { get; private set; } // وضعیت جاری (داده مرجع case_statuses)
        public UserId? AssignedExpertId { get; private set; }     // کارشناس جاری
        public LookupId? AssignmentSourceId { get; private set; } // منبع تخصیص (lookup: assignment_source)
        public DateTime? AssignedAt { get; private set; }
        public DateTime? ResponseDeadline { get; private set; }   // مهلت پاسخ کارشناس (SLA)
        public CommissionRate? CommissionRate { get; private set; } // NULL = پیش‌فرض تنظیمات
        public DateTime? StatusChangedAt { get; private set; }
        public string? Meta { get; private set; }                 // jsonb آزاد (اعتبارسنجی در لایه Application)
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        private readonly List<CaseStatusLog> _statusLogs = new();
        public IReadOnlyCollection<CaseStatusLog> StatusLogs => _statusLogs.AsReadOnly();

        private readonly List<CaseAssignment> _assignments = new();
        public IReadOnlyCollection<CaseAssignment> Assignments => _assignments.AsReadOnly();

        private readonly List<CaseNote> _notes = new();
        public IReadOnlyCollection<CaseNote> Notes => _notes.AsReadOnly();

        // آیا تخصیصی بازِ در انتظار پاسخ وجود دارد؟ (برای صف کارشناس و کنترل تخصیص مجدد)
        public bool HasOpenAssignment => _assignments.Any(a => a.IsOpen);

        // آیا مهلت پاسخ (SLA) منقضی شده؟ — ورودی Job زمان‌بندِ لایه Application
        public bool IsResponseDeadlinePassed(DateTime nowUtc)
            => ResponseDeadline is not null && nowUtc > ResponseDeadline.Value;

        private Case() { } // برای EF Core

        private Case(
            CaseId id,
            TrackingCode trackingCode,
            UserId farmerId,
            PlotId plotId,
            CropId cropId,
            SeasonYear seasonYear,
            Area area,
            CaseStatusId currentStatusId)
        {
            Id = id;
            TrackingCode = trackingCode;
            FarmerId = farmerId;
            PlotId = plotId;
            CropId = cropId;
            SeasonYear = seasonYear;
            Area = area;
            CurrentStatusId = currentStatusId;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = CreatedAt;
        }

        // ثبت پرونده جدید در پذیرش (intake) — نخستین رکورد تاریخچه وضعیت هم همین‌جا مهر می‌شود
        public static Result<Case> Create(
            CaseId id,
            TrackingCode trackingCode,
            UserId farmerId,
            PlotId plotId,
            CropId cropId,
            SeasonYear seasonYear,
            Area area,
            CaseStatusId initialStatusId,
            CaseStatusLogId initialStatusLogId,
            VarietyId? varietyId = null,
            DateOnly? plantingDate = null,
            CommissionRate? commissionRate = null,
            string? meta = null)
        {
            if (id is null || trackingCode is null || farmerId is null || plotId is null
                || cropId is null || seasonYear is null || area is null
                || initialStatusId is null || initialStatusLogId is null)
                return Result.Failure<Case>(CaseErrors.NullReferenceError());

            var kase = new Case(id, trackingCode, farmerId, plotId, cropId, seasonYear, area, initialStatusId);
            kase.VarietyId = varietyId;
            kase.PlantingDate = plantingDate;
            kase.CommissionRate = commissionRate;
            kase.Meta = meta;

            // نخستین وضعیت توسط سیستم ثبت می‌شود (FromStatusId = NULL، بدون کاربر)
            kase._statusLogs.Add(CaseStatusLog.Create(
                initialStatusLogId, null, initialStatusId, null, isAutomatic: true, note: null));

            kase.AddDomainEvent(new CaseCreatedEvent(
                id.Value, trackingCode.Value, farmerId.Value, plotId.Value,
                cropId.Value, varietyId?.Value, seasonYear.Year, area.Hectares));
            return Result<Case>.Success(kase);
        }

        // تغییر وضعیت پرونده — همراه با ثبت اجباری در تاریخچه
        // بررسی «مجاز بودن این گذار برای این نقش» بر اساس جدول case_status_transitions
        // در لایه Application انجام می‌شود؛ اینجا فقط ناسازگاری‌های درون‌Aggregate بسته می‌شود.
        public Result ChangeStatus(
            CaseStatusLogId statusLogId,
            CaseStatusId toStatusId,
            UserId? changedBy = null,
            bool isAutomatic = false,
            string? note = null)
        {
            if (statusLogId is null || toStatusId is null)
                return Result.Failure(CaseErrors.NullReferenceError());

            if (toStatusId.Value == CurrentStatusId.Value)
                return Result.Failure(CaseErrors.AlreadyInStatus(Id.Value, toStatusId.Value));

            var fromStatusId = CurrentStatusId;

            _statusLogs.Add(CaseStatusLog.Create(
                statusLogId, fromStatusId, toStatusId, changedBy, isAutomatic, note));

            CurrentStatusId = toStatusId;
            StatusChangedAt = DateTime.UtcNow;
            UpdatedAt = StatusChangedAt.Value;

            AddDomainEvent(new CaseStatusChangedEvent(
                Id.Value, fromStatusId.Value, toStatusId.Value, changedBy?.Value, isAutomatic));
            return Result.Success();
        }

        // تخصیص پرونده به کارشناس با مهلت پاسخ (SLA)
        // تا وقتی تخصیص بازی وجود دارد، تخصیص جدید ممکن نیست (اول نتیجه تخصیص فعلی را ثبت کنید)
        public Result AssignExpert(
            CaseAssignmentId assignmentId,
            UserId expertId,
            DateTime deadline,
            LookupId? sourceId = null,
            DateTime? assignedAt = null,
            string? note = null)
        {
            if (assignmentId is null || expertId is null)
                return Result.Failure(CaseErrors.NullReferenceError());

            var at = assignedAt ?? DateTime.UtcNow;
            if (deadline <= at)
                return Result.Failure(CaseErrors.InvalidDeadline(deadline));

            if (HasOpenAssignment)
                return Result.Failure(CaseErrors.OpenAssignmentExists(Id.Value));

            _assignments.Add(CaseAssignment.Create(assignmentId, expertId, at, deadline, sourceId, note));

            AssignedExpertId = expertId;
            AssignmentSourceId = sourceId;
            AssignedAt = at;
            ResponseDeadline = deadline;
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new CaseExpertAssignedEvent(
                Id.Value, assignmentId.Value, expertId.Value, deadline));
            return Result.Success();
        }

        // ثبت نتیجه پاسخ کارشناس به تخصیص باز (پذیرش/ردی/...) — SLA پرونده بسته می‌شود
        // تصمیم بعدی (مانند خروج کارشناس از پرونده) با UnassignExpert در گام بعدی انجام می‌شود.
        public Result RecordAssignmentResponse(LookupId outcomeId, DateTime respondedAt, string? note = null)
        {
            if (outcomeId is null)
                return Result.Failure(CaseErrors.NullReferenceError());

            var openAssignment = _assignments.LastOrDefault(a => a.IsOpen);
            if (openAssignment is null)
                return Result.Failure(CaseErrors.NoOpenAssignment(Id.Value));

            var response = openAssignment.RecordResponse(outcomeId, respondedAt, note);
            if (response.IsFailure)
                return response;

            ResponseDeadline = null;
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new CaseAssignmentRespondedEvent(
                Id.Value, openAssignment.Id.Value, openAssignment.ExpertId.Value,
                outcomeId.Value, respondedAt));
            return Result.Success();
        }

        // خروج کارشناس از پرونده (بازگشت به صف) — فقط بعد از بسته‌شدن تخصیص باز
        public Result UnassignExpert()
        {
            if (AssignedExpertId is null)
                return Result.Failure(CaseErrors.NoAssignedExpert(Id.Value));

            if (HasOpenAssignment)
                return Result.Failure(CaseErrors.OpenAssignmentExists(Id.Value));

            var expertId = AssignedExpertId;

            AssignedExpertId = null;
            AssignmentSourceId = null;
            AssignedAt = null;
            ResponseDeadline = null;
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new CaseExpertUnassignedEvent(Id.Value, expertId.Value));
            return Result.Success();
        }

        // تغییر مهلت پاسخ (تمدید یا تقلیل SLA جاری) — نیاز به کارشناس تخصیص‌یافته دارد
        public Result ChangeResponseDeadline(DateTime newDeadline)
        {
            if (AssignedExpertId is null || ResponseDeadline is null)
                return Result.Failure(CaseErrors.NoAssignedExpert(Id.Value));

            if (newDeadline <= DateTime.UtcNow)
                return Result.Failure(CaseErrors.InvalidDeadline(newDeadline));

            var oldDeadline = ResponseDeadline.Value;
            ResponseDeadline = newDeadline;
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new CaseResponseDeadlineChangedEvent(
                Id.Value, oldDeadline, newDeadline));
            return Result.Success();
        }

        // افزودن یادداشت آزاد به پرونده
        public Result AddNote(CaseNoteId noteId, UserId authorId, string body)
        {
            if (noteId is null || authorId is null)
                return Result.Failure(CaseErrors.NullReferenceError());

            if (string.IsNullOrWhiteSpace(body))
                return Result.Failure(CaseErrors.InvalidNote());

            var trimmedBody = body.Trim();
            if (trimmedBody.Length > CaseNote.MaxBodyLength)
                return Result.Failure(CaseErrors.NoteTooLong(trimmedBody.Length));

            _notes.Add(CaseNote.Create(noteId, authorId, trimmedBody));
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new CaseNoteAddedEvent(Id.Value, noteId.Value, authorId.Value));
            return Result.Success();
        }

        // تعیین/اصلاح تاریخ کشت (در پذیرش ممکن است خالی باشد و بعداً ثبت شود)
        public Result SetPlantingDate(DateOnly? plantingDate)
        {
            PlantingDate = plantingDate;
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new CasePlantingDateSetEvent(Id.Value, plantingDate));
            return Result.Success();
        }

        // تعیین کارمزد اختصاصی پرونده؛ مقدار تهی یعنی بازگشت به کارمزد پیش‌فرض تنظیمات
        public Result SetCommissionRate(CommissionRate? commissionRate)
        {
            CommissionRate = commissionRate;
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new CaseCommissionRateChangedEvent(Id.Value, commissionRate?.Rate));
            return Result.Success();
        }
    }
}
