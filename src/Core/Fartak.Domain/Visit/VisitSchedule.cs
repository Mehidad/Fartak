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

namespace Fartak.Domain.Visit
{

    // زمان‌بندی بازدید (جدول visit_schedules) — بازدید اولیه/دوره‌ای/پیگیری/ویژه؛
    // مبنای تولید یادآور برای کارشناس (reminders) و نقطه اتصال به بازدید میدانی (field_visits).
    //
    // تعلق تغییرناپذیر: پرونده، نوع بازدید و ایجادکننده پس از ایجاد تغییر نمی‌کنند —
    // این زمان‌بندی مال همان پرونده با همان نوع است؛ اگر نوع اشتباه ثبت شده، لغو و زمان‌بندی جدید.
    //
    // وضعیت بازدید با LookupId (domain=visit_status) نگه داشته می‌شود؛ اینکه کدام کدها «بسته»اند
    // (انجام‌شده/لغوشده) و گذار مجاز از وضعیت بسته، داده lookup است و در لایه Application
    // با بارگذاری این تجمع کنترل می‌شود (فلسفه lookup-data-driven).
    //
    // قاعده زمان: درون‌ریزی داده تاریخی مجاز است (Create زمان گذشته می‌پذیرد) اما
    // تغییر زمان‌بندی (Reschedule) به زمان گذشته همیشه خطای ورود داده است و در دامنه رد می‌شود.
    //
    // قالب چک‌لیست و یادداشت تا قبل از انجام بازدید قابل تغییرند؛ اینکه بازدید انجام شده یا نه
    // (وجود field_visit برای این زمان‌بندی) در لایه Application بررسی می‌شود.
    //
    // جدول ستون updated_at ندارد — به‌روزرسانی‌ها فقط از طریق رویدادها ردیابی می‌شوند.
    public class VisitSchedule : AggregateRoot<VisitScheduleId>
    {
        // سقف عملیاتی یادداشت زمان‌بندی (ستون note از نوع text است و در DB سقف ندارد)
        public const int MaxNoteLength = 1000;

        public CaseId CaseId { get; private set; }                        // پرونده صاحب زمان‌بندی — immutable
        public LookupId VisitTypeId { get; private set; }                 // lookup: visit_type (اولیه/دوره‌ای/پیگیری/ویژه) — immutable
        public ChecklistTemplateId? ChecklistTemplateId { get; private set; }  // قالب چک‌لیست پیشنهادی برای بازدید — قابل تغییر
        public DateTime ScheduledFor { get; private set; }                // زمان برنامه‌ریزی‌شده بازدید (UTC)
        public LookupId StatusId { get; private set; }                    // lookup: visit_status (زمان‌بندی‌شده/انجام‌شده/...)
        public UserId? CreatedBy { get; private set; }                    // ایجادکننده؛ تهی = زمان‌بندی خودکار سیستم — immutable
        public DateTime CreatedAt { get; private set; }
        public string? Note { get; private set; }

        private VisitSchedule() { } // برای EF Core

        private VisitSchedule(VisitScheduleId id, CaseId caseId, LookupId visitTypeId,
            ChecklistTemplateId? checklistTemplateId, DateTime scheduledFor, LookupId statusId, UserId? createdBy)
        {
            Id = id;
            CaseId = caseId;
            VisitTypeId = visitTypeId;
            ChecklistTemplateId = checklistTemplateId;
            ScheduledFor = scheduledFor;
            StatusId = statusId;
            CreatedBy = createdBy;
            CreatedAt = DateTime.UtcNow;
        }

        public static Result<VisitSchedule> Create(
            VisitScheduleId id,
            CaseId caseId,
            LookupId visitTypeId,
            DateTime scheduledFor,
            LookupId statusId,
            UserId? createdBy = null,
            ChecklistTemplateId? checklistTemplateId = null,
            string? note = null)
        {
            if (id is null || caseId is null || visitTypeId is null || statusId is null)
                return Result.Failure<VisitSchedule>(VisitScheduleErrors.NullReferenceError());

            var noteValidation = ValidateNote(note);
            if (noteValidation.IsFailure)
                return Result.Failure<VisitSchedule>(noteValidation.Error);

            var schedule = new VisitSchedule(
                id, caseId, visitTypeId, checklistTemplateId, scheduledFor, statusId, createdBy);
            schedule.Note = NormalizeNote(note);

            schedule.AddDomainEvent(new VisitScheduleCreatedEvent(
                id.Value, caseId.Value, visitTypeId.Value, checklistTemplateId?.Value,
                scheduledFor, statusId.Value, createdBy?.Value));
            return Result<VisitSchedule>.Success(schedule);
        }

        // تغییر زمان بازدید — به زمان گذشته رد می‌شود؛
        // اینکه وضعیت فعلی اجازه تغییر زمان می‌دهد یا نه (وضعیت بسته) در لایه Application کنترل می‌شود
        public Result Reschedule(DateTime newScheduledFor)
        {
            if (newScheduledFor <= DateTime.UtcNow)
                return Result.Failure(VisitScheduleErrors.ScheduledForInPast(newScheduledFor));

            if (newScheduledFor == ScheduledFor)
                return Result.Failure(VisitScheduleErrors.ScheduledForUnchanged());

            var oldScheduledFor = ScheduledFor;
            ScheduledFor = newScheduledFor;
            AddDomainEvent(new VisitScheduleRescheduledEvent(
                Id.Value, CaseId.Value, oldScheduledFor, newScheduledFor));
            return Result.Success();
        }

        // تغییر وضعیت زمان‌بندی (مثلاً لغو یا به‌تعویق) —
        // گذارهای مجاز وضعیت با کدهای lookup در لایه Application کنترل می‌شود
        public Result ChangeStatus(LookupId newStatusId)
        {
            if (newStatusId is null)
                return Result.Failure(VisitScheduleErrors.NullReferenceError());

            if (newStatusId.Value == StatusId.Value)
                return Result.Failure(VisitScheduleErrors.StatusUnchanged());

            var oldStatusId = StatusId;
            StatusId = newStatusId;
            AddDomainEvent(new VisitScheduleStatusChangedEvent(
                Id.Value, CaseId.Value, oldStatusId.Value, newStatusId.Value));
            return Result.Success();
        }

        // تعیین/تغییر/پاک‌کردن قالب چک‌لیست بازدید — مقدار تهی یعنی بدون قالب مشخص؛
        // تغییر قالب برای بازدیدِ انجام‌شده در لایه Application مسدود می‌شود
        public Result SetChecklistTemplate(ChecklistTemplateId? newChecklistTemplateId)
        {
            if (newChecklistTemplateId?.Value == ChecklistTemplateId?.Value)
                return Result.Failure(VisitScheduleErrors.ChecklistTemplateUnchanged());

            var oldChecklistTemplateId = ChecklistTemplateId;
            ChecklistTemplateId = newChecklistTemplateId;
            AddDomainEvent(new VisitScheduleChecklistTemplateChangedEvent(
                Id.Value, CaseId.Value, oldChecklistTemplateId?.Value, newChecklistTemplateId?.Value));
            return Result.Success();
        }

        // به‌روزرسانی یادداشت زمان‌بندی — مقدار تهی یعنی پاک‌کردن یادداشت
        public Result SetNote(string? note)
        {
            var noteValidation = ValidateNote(note);
            if (noteValidation.IsFailure)
                return noteValidation;

            var normalized = NormalizeNote(note);
            if (normalized == Note)
                return Result.Failure(VisitScheduleErrors.NoteUnchanged());

            Note = normalized;
            AddDomainEvent(new VisitScheduleNoteChangedEvent(Id.Value, CaseId.Value));
            return Result.Success();
        }

        private static Result ValidateNote(string? note)
        {
            if (string.IsNullOrWhiteSpace(note))
                return Result.Success(); // اختیاری است

            if (note.Trim().Length > MaxNoteLength)
                return Result.Failure(VisitScheduleErrors.NoteTooLong(note.Trim()));

            return Result.Success();
        }

        private static string? NormalizeNote(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
