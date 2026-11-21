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
using Fartak.Domain.VisitSchedule.Events;

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
    // VisitSchedule.cs
   
    public sealed class VisitSchedule : AggregateRoot<VisitScheduleId>
    {
        public const int NoteMaxLength = 500;

        public CaseId CaseId { get; private set; }
        public LookupId VisitTypeId { get; private set; } // نوع بازدید: پیش‌کاشت، حین رشد، برداشت، کارشناسی خسارت و ...
        public ChecklistTemplateId? ChecklistTemplateId { get; private set; }
        public DateTime ScheduledFor { get; private set; }
        public LookupId StatusId { get; private set; } // وضعیت: برنامه‌ریزی‌شده، انجام‌شده، جابجا‌شده، لغو‌شده و ...
        public UserId? CreatedBy { get; private set; }
        public string? Note { get; private set; }

        private VisitSchedule()
        {
            // برای EF Core
        }

        private VisitSchedule(
            VisitScheduleId id,
            CaseId caseId,
            LookupId visitTypeId,
            ChecklistTemplateId? checklistTemplateId,
            DateTime scheduledFor,
            LookupId statusId,
            UserId? createdBy)
            : base(id)
        {
            CaseId = caseId;
            VisitTypeId = visitTypeId;
            ChecklistTemplateId = checklistTemplateId;
            ScheduledFor = scheduledFor;
            StatusId = statusId;
            CreatedBy = createdBy;
        }

        public static Result<VisitSchedule> Create(
            VisitScheduleId id,
            CaseId caseId,
            LookupId visitTypeId,
            ChecklistTemplateId? checklistTemplateId,
            DateTime scheduledFor,
            LookupId statusId,
            UserId? createdBy)
        {
            if (caseId == CaseId.Empty)
                return Result.Failure<VisitSchedule>(VisitScheduleErrors.CaseNotSpecified);

            if (visitTypeId == LookupId.Empty)
                return Result.Failure<VisitSchedule>(VisitScheduleErrors.VisitTypeNotSpecified);

            if (statusId == LookupId.Empty)
                return Result.Failure<VisitSchedule>(VisitScheduleErrors.StatusNotSpecified);

            if (scheduledFor < DateTime.UtcNow)
                return Result.Failure<VisitSchedule>(VisitScheduleErrors.ScheduledForInPast);

            var schedule = new VisitSchedule(id, caseId, visitTypeId, checklistTemplateId, scheduledFor, statusId, createdBy);

            schedule.AddDomainEvent(new Events.VisitScheduleCreatedEvent(
                id.Value,
                caseId.Value,
                visitTypeId.Value,
                checklistTemplateId?.Value,
                scheduledFor,
                statusId.Value,
                createdBy?.Value));

            return Result.Success(schedule);
        }

        public Result ChangeVisitType(LookupId newVisitTypeId)
        {
            if (newVisitTypeId == LookupId.Empty)
                return Result.Failure(VisitScheduleErrors.VisitTypeNotSpecified);

            if (newVisitTypeId == VisitTypeId)
                return Result.Failure(VisitScheduleErrors.SameVisitType);

            var oldVisitTypeId = VisitTypeId;
            VisitTypeId = newVisitTypeId;

            AddDomainEvent(new VisitScheduleVisitTypeChangedEvent(
                Id.Value,
                CaseId.Value,
                oldVisitTypeId.Value,
                newVisitTypeId.Value));

            return Result.Success();
        }

        public Result ChangeStatus(LookupId newStatusId)
        {
            if (newStatusId == LookupId.Empty)
                return Result.Failure(VisitScheduleErrors.StatusNotSpecified);

            if (newStatusId == StatusId)
                return Result.Failure(VisitScheduleErrors.SameStatus);

            var oldStatusId = StatusId;
            StatusId = newStatusId;

            AddDomainEvent(new Events.VisitScheduleStatusChangedEvent(
                Id.Value,
                CaseId.Value,
                oldStatusId.Value,
                newStatusId.Value));

            return Result.Success();
        }

        public Result Reschedule(DateTime newScheduledFor)
        {
            if (newScheduledFor < DateTime.UtcNow)
                return Result.Failure(VisitScheduleErrors.NewScheduledForInPast);

            if (newScheduledFor == ScheduledFor)
                return Result.Failure(VisitScheduleErrors.SameScheduledFor);

            var oldScheduledFor = ScheduledFor;
            ScheduledFor = newScheduledFor;

            AddDomainEvent(new Events.VisitScheduleRescheduledEvent(
                Id.Value,
                CaseId.Value,
                oldScheduledFor,
                newScheduledFor));

            return Result.Success();
        }

        public Result SetNote(string? note)
        {
            var trimmedNote = note?.Trim();
            if (trimmedNote is not null && trimmedNote.Length > NoteMaxLength)
                return Result.Failure(VisitScheduleErrors.NoteTooLong);

            var oldNote = Note;
            Note = trimmedNote;

            AddDomainEvent(new Events.VisitScheduleNoteChangedEvent(
                Id.Value,
                CaseId.Value,
                oldNote,
                trimmedNote));

            return Result.Success();
        }

        public Result SetChecklistTemplate(ChecklistTemplateId? checklistTemplateId)
        {
            ChecklistTemplateId = checklistTemplateId;

            AddDomainEvent(new VisitScheduleChecklistTemplateSetEvent(
                Id.Value,
                CaseId.Value,
                checklistTemplateId?.Value));

            return Result.Success();
        }
    }
}
