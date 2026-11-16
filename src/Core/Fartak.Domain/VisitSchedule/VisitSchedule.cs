using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Case;
using Fartak.Domain.Checklist;
using Fartak.Domain.Common;
using Fartak.Domain.Framework;
using Fartak.Domain.User.Fartak.Domain.User;
using Fartak.Domain.Visit;
using Fartak.Domain.Visit.Events;

namespace Fartak.Domain.VisitSchedule
{
    // زمان‌بندی بازدید میدانی (جدول visit_schedules) — تجمع مستقل؛
    // رابطه با بازدیدِ انجام‌شده در Aggregate «FieldVisit» ساکن است (field_visits.visit_schedule_id)
    // و اینجا هیچ اشاره‌ای به بازدیدها نگه نمی‌داریم.
    //
    // ستون‌های جدول: case_id، visit_type_id (lookup: visit_type)، checklist_template_id (اختیاری)؛
    // scheduled_for، status_id (lookup: visit_status)، created_by (اختیاری)، created_at و note.
    // طبق قاعده timestamp/is_active: created_at داریم ولی updated_at نداریم → فقط CreatedAt؛
    // is_active نداریم → متد Activate/Deactivate هم ندارد؛ چرخه حیات با status_id مدیریت می‌شود.
    //
    // کدهای مرجع visit_status در اسکیما: planned / done / canceled (املای مرجع ERD) / expired.
    // ماشین وضعیت: planned → done | canceled | expired و هر سه پایانیِ بسته‌اند؛
    // «منقضی‌شده» را Job یادآورِ لایه Application می‌زند و «زمان‌بندی دوبارهٔ» یک رکوردِ بسته
    // با رکورد جدید انجام می‌شود، نه با بازکردن رکورد قدیمی (حفظ تاریخچه یادآورها).
    //
    // قاعده ارسال کد وضعیت: تجمع فقط LookupId را نگه می‌دارد؛ برای قواعد رفتاری، کدِ وضعیتِ
    // جاری و جدید از کش lookup در لایه Application کنار شناسه ارسال می‌شود — همان الگوی
    // کدِ answer_type در Aggregate FieldVisit. خودِ گذارهای مجاز، قاعدهٔ این دامنه‌اند و اینجا بسته می‌شوند.
    //
    // تغییرناپذیرها پس از ایجاد: CaseId و CreatedBy؛ تغییر زمان فقط با Reschedule و فقط در وضعیت باز.
    // case_id تغییر نمی‌کند — زمان‌بندی برای پروندهٔ دیگر یعنی رکورد جدید.
    public class VisitSchedule : AggregateRoot<VisitScheduleId>
    {
        public const int MaxNoteLength = 2000;      // ستون note از نوع text است؛ سقف در دامنه بسته می‌شود
        private const int PastToleranceMinutes = 5; // گذرگاه ساعت/تاخیر شبکه — قرینه FutureToleranceMinutes در FieldVisit

        public CaseId CaseId { get; private set; }                          // پروندهٔ صاحب زمان‌بندی (تغییرناپذیر)
        public LookupId VisitTypeId { get; private set; }                   // نوع بازدید (lookup: visit_type — initial/periodic/followup/special)
        public ChecklistTemplateId? ChecklistTemplateId { get; private set; } // قالب چک‌لیستِ این بازدید (اختیاری)
        public DateTime ScheduledFor { get; private set; }                  // زمان بازدید (UTC)
        public LookupId StatusId { get; private set; }                      // وضعیت جاری (lookup: visit_status)
        public UserId? CreatedBy { get; private set; }                      // چه کسی زمان‌بندی کرد (NULL = سیستم/Job)
        public DateTime CreatedAt { get; private set; }
        public string? Note { get; private set; }                           // یادداشت هماهنگی/دلیل لغو

        private VisitSchedule() { } // برای EF Core

        private VisitSchedule(
            VisitScheduleId id,
            CaseId caseId,
            LookupId visitTypeId,
            DateTime scheduledFor,
            LookupId statusId)
        {
            Id = id;
            CaseId = caseId;
            VisitTypeId = visitTypeId;
            ScheduledFor = scheduledFor;
            StatusId = statusId;
            CreatedAt = DateTime.UtcNow;
        }

        // ثبت زمان‌بندی جدید — همیشه با وضعیت آغازین «برنامه‌ریزی‌شده» (planned)
        public static Result<VisitSchedule> Create(
            VisitScheduleId id,
            CaseId caseId,
            LookupId visitTypeId,
            DateTime scheduledFor,
            LookupId initialStatusId,
            string initialStatusCode,
            ChecklistTemplateId? checklistTemplateId = null,
            UserId? createdBy = null,
            string? note = null)
        {
            if (id is null || caseId is null || visitTypeId is null
                || initialStatusId is null || initialStatusCode is null)
                return Result.Failure<VisitSchedule>(VisitScheduleErrors.NullReferenceError());

            var normalizedStatusCode = initialStatusCode.Trim();
            if (!IsKnownStatusCode(normalizedStatusCode))
                return Result.Failure<VisitSchedule>(VisitScheduleErrors.UnknownStatusCode(normalizedStatusCode));

            if (normalizedStatusCode != WellKnownStatuses.Planned)
                return Result.Failure<VisitSchedule>(VisitScheduleErrors.InvalidInitialStatus(normalizedStatusCode));

            if (scheduledFor.AddMinutes(PastToleranceMinutes) < DateTime.UtcNow)
                return Result.Failure<VisitSchedule>(VisitScheduleErrors.ScheduledForInPast(scheduledFor));

            var normalizedNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
            if (normalizedNote is not null && normalizedNote.Length > MaxNoteLength)
                return Result.Failure<VisitSchedule>(VisitScheduleErrors.NoteTooLong(normalizedNote.Length));

            var schedule = new VisitSchedule(id, caseId, visitTypeId, scheduledFor, initialStatusId);
            schedule.ChecklistTemplateId = checklistTemplateId;
            schedule.CreatedBy = createdBy;
            schedule.Note = normalizedNote;

            schedule.AddDomainEvent(new VisitScheduleCreatedEvent(
                id.Value, caseId.Value, visitTypeId.Value, scheduledFor));
            return Result<VisitSchedule>.Success(schedule);
        }

        // جابه‌جایی زمان بازدید — فقط روی رکورد باز (planned)؛ وضعیت عوض نمی‌شود
        public Result Reschedule(DateTime newScheduledFor, string currentStatusCode)
        {
            var openCheck = EnsureOpen(currentStatusCode);
            if (openCheck.IsFailure)
                return openCheck;

            if (newScheduledFor.AddMinutes(PastToleranceMinutes) < DateTime.UtcNow)
                return Result.Failure(VisitScheduleErrors.ScheduledForInPast(newScheduledFor));

            if (newScheduledFor == ScheduledFor)
                return Result.Failure(VisitScheduleErrors.ScheduledForUnchanged(newScheduledFor));

            var oldScheduledFor = ScheduledFor;
            ScheduledFor = newScheduledFor;

            AddDomainEvent(new VisitScheduleRescheduledEvent(Id.Value, oldScheduledFor, newScheduledFor));
            return Result.Success();
        }

        // تغییر نوع بازدید (مثلاً پیگیری → ویژه) — فقط روی رکورد باز
        public Result ChangeVisitType(LookupId newVisitTypeId, string currentStatusCode)
        {
            if (newVisitTypeId is null)
                return Result.Failure(VisitScheduleErrors.NullReferenceError());

            var openCheck = EnsureOpen(currentStatusCode);
            if (openCheck.IsFailure)
                return openCheck;

            if (newVisitTypeId.Value == VisitTypeId.Value)
                return Result.Failure(VisitScheduleErrors.VisitTypeUnchanged(newVisitTypeId.Value));

            var oldVisitTypeId = VisitTypeId;
            VisitTypeId = newVisitTypeId;

            AddDomainEvent(new VisitScheduleVisitTypeChangedEvent(
                Id.Value, oldVisitTypeId.Value, newVisitTypeId.Value));
            return Result.Success();
        }

        // تغییر وضعیت چرخه حیات: انجام‌شدن، لغو یا منقضی‌شدن (expired را Job یادآور می‌زند)
        public Result ChangeStatus(LookupId newStatusId, string newStatusCode, string currentStatusCode)
        {
            if (newStatusId is null || string.IsNullOrWhiteSpace(newStatusCode) || string.IsNullOrWhiteSpace(currentStatusCode))
                return Result.Failure(VisitScheduleErrors.NullReferenceError());

            var normalizedNew = newStatusCode.Trim();
            var normalizedCurrent = currentStatusCode.Trim();

            if (!IsKnownStatusCode(normalizedNew))
                return Result.Failure(VisitScheduleErrors.UnknownStatusCode(normalizedNew));

            if (!IsKnownStatusCode(normalizedCurrent))
                return Result.Failure(VisitScheduleErrors.UnknownStatusCode(normalizedCurrent));

            if (newStatusId.Value == StatusId.Value)
                return Result.Failure(VisitScheduleErrors.StatusUnchanged(Id.Value, newStatusId.Value));

            if (!AllowedTransitions[normalizedCurrent].Contains(normalizedNew))
                return Result.Failure(VisitScheduleErrors.InvalidStatusTransition(normalizedCurrent, normalizedNew));

            var fromStatusId = StatusId;
            StatusId = newStatusId;

            AddDomainEvent(new VisitScheduleStatusChangedEvent(
                Id.Value, fromStatusId.Value, newStatusId.Value, normalizedNew));
            return Result.Success();
        }

        // اتصال/تعویض/خالی‌کردن قالب چک‌لیست — فقط روی رکورد باز؛ null یعنی حذف قالب
        public Result SetChecklistTemplate(ChecklistTemplateId? checklistTemplateId, string currentStatusCode)
        {
            var openCheck = EnsureOpen(currentStatusCode);
            if (openCheck.IsFailure)
                return openCheck;

            var newTemplateId = checklistTemplateId?.Value;
            var oldTemplateId = ChecklistTemplateId?.Value;
            if (newTemplateId == oldTemplateId)
                return Result.Failure(VisitScheduleErrors.ChecklistTemplateUnchanged(newTemplateId));

            ChecklistTemplateId = checklistTemplateId;
            AddDomainEvent(new VisitScheduleChecklistTemplateSetEvent(Id.Value, newTemplateId));
            return Result.Success();
        }

        // یادداشت هماهنگی — در همهٔ وضعیت‌ها قابل ویرایش است
        // (مثلاً ثبت «دلیل لغو» بعد از لغو)؛ با null پاک‌سازی می‌شود
        public Result ChangeNote(string? note)
        {
            var normalizedNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
            if (normalizedNote is not null && normalizedNote.Length > MaxNoteLength)
                return Result.Failure(VisitScheduleErrors.NoteTooLong(normalizedNote.Length));

            if (normalizedNote == Note)
                return Result.Failure(VisitScheduleErrors.NoteUnchanged());

            Note = normalizedNote;
            AddDomainEvent(new VisitScheduleNoteChangedEvent(Id.Value));
            return Result.Success();
        }

        // کدهای وضعیت مرجع visit_status در اسکیما — مثل WellKnownCodes در Aggregate Role
        public static class WellKnownStatuses
        {
            public const string Planned = "planned";    // برنامه‌ریزی‌شده — تنها وضعیت باز
            public const string Done = "done";          // انجام‌شده
            public const string Canceled = "canceled";  // لغوشده (املای مرجع ERD — یک L)
            public const string Expired = "expired";    // منقضی‌شده — توسط Job یادآورِ Application
        }

        // گذارهای مجاز ماشین وضعیت — قاعدهٔ دامنه؛ منقضی/انجام/لغو همه پایانی‌اند
        private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> AllowedTransitions =
            new Dictionary<string, IReadOnlyCollection<string>>
            {
                [WellKnownStatuses.Planned] = new[]
                {
                    WellKnownStatuses.Done,
                    WellKnownStatuses.Canceled,
                    WellKnownStatuses.Expired
                },
                [WellKnownStatuses.Done] = Array.Empty<string>(),
                [WellKnownStatuses.Canceled] = Array.Empty<string>(),
                [WellKnownStatuses.Expired] = Array.Empty<string>()
            };

        private static bool IsKnownStatusCode(string code)
            => code == WellKnownStatuses.Planned
            || code == WellKnownStatuses.Done
            || code == WellKnownStatuses.Canceled
            || code == WellKnownStatuses.Expired;

        // اعتبارسنجی کد وضعیت جاری + کنترل باز بودن رکورد — پیش‌نیاز هر تغییر روی زمان‌بندی
        private static Result EnsureOpen(string? currentStatusCode)
        {
            if (string.IsNullOrWhiteSpace(currentStatusCode))
                return Result.Failure(VisitScheduleErrors.NullReferenceError());

            var normalized = currentStatusCode.Trim();
            if (!IsKnownStatusCode(normalized))
                return Result.Failure(VisitScheduleErrors.UnknownStatusCode(normalized));

            if (normalized != WellKnownStatuses.Planned)
                return Result.Failure(VisitScheduleErrors.ScheduleNotOpen(normalized));

            return Result.Success();
        }
    }
}
