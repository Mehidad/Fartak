using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Fartak.Domain.Case;
using Fartak.Domain.Checklist;
using Fartak.Domain.Common;
using Fartak.Domain.Framework;
using Fartak.Domain.Unit;
using Fartak.Domain.User.Fartak.Domain.User;

namespace Fartak.Domain.FieldVisit
{
    // بازدید میدانی انجام‌شده + جمع‌بندی کلی (جدول field_visits) همراه با پاسخ‌های چک‌لیست آن
    // (جدول visit_answers) — دو جدول = یک تجمع: پاسخ‌ها جزء تعریف بازدیدند (FK اجباری + ON DELETE CASCADE).
    // جدول field_visits ستون updated_at و is_active ندارد → نه UpdatedAt داریم و نه Activate/Deactivate.
    // case_id و expert_id فیلد ماهوی بازدیدند و پس از ایجاد تغییر نمی‌کنند؛
    // visit_schedule_id صرفاً «ارجاع به برنامهٔ مادر» است و با LinkToSchedule قابل تغییر است.
    // عکس‌های بازدید در تجمع attachments (با entity_type=field_visit) نگه داشته می‌شود و اینجا نیست؛
    // توصیه‌های بعد از بازدید (case_recommendations) نیز تجمع مستقل خودشان هستند.
    public class FieldVisit : AggregateRoot<FieldVisitId>
    {
        // سقف عملیاتی جمع‌بندی کلی (ستون overall_summary از نوع text است و در DB سقف ندارد)
        public const int MaxSummaryLength = 4000;
        // اعشار مجاز مقدار عددی پاسخ — هم‌خوان با numeric(12,3) در visit_answers
        public const int NumericValueScale = 3;
        public const decimal MaxNumericValue = 999_999_999.999m;
        public const decimal MinNumericValue = -999_999_999.999m;
        // تلورانس ساعت برای «آینده» — بازدید ثبت‌شده در آینده معنا ندارد
        public const int FutureToleranceMinutes = 5;

        // کدهای شناخته‌شدهٔ دامنه answer_type در lookups (عضویت اجباری نیست؛ هم‌خوانی نوع پاسخ
        // با خانهٔ مقدار متناظر در لایه Application با خواندن code از lookups کنترل می‌شود).
        // دامنه rating هم در seed با کدهای good/medium/weak/critical پر می‌شود.
        public static class WellKnownAnswerTypes
        {
            public const string Rating = "rating";    // مقیاس کیفی
            public const string Boolean = "boolean";  // بله/خیر
            public const string Numeric = "numeric";  // عددی
            public const string Text = "text";        // متنی
        }

        public CaseId CaseId { get; private set; }                     // پروندهٔ مادر — تغییرناپذیر
        public VisitScheduleId? VisitScheduleId { get; private set; }  // برنامهٔ بازدید مادر (اختیاری)
        public UserId ExpertId { get; private set; }                   // کارشناس بازدیدکننده — تغییرناپذیر
        public DateTime VisitedAt { get; private set; }                // زمان انجام بازدید
        public LookupId? GrowthStageId { get; private set; }           // مرحلهٔ رشد مشاهده‌شده (دامنه growth_stage)
        public string? OverallSummary { get; private set; }            // جمع‌بندی کلی بازدید
        public DateTime CreatedAt { get; private set; }

        private readonly List<VisitAnswer> _answers = new();
        public IReadOnlyCollection<VisitAnswer> Answers => _answers.AsReadOnly();

        private FieldVisit() { } // برای EF Core

        private FieldVisit(
            FieldVisitId id,
            CaseId caseId,
            VisitScheduleId? visitScheduleId,
            UserId expertId,
            DateTime visitedAt,
            LookupId? growthStageId,
            string? overallSummary)
        {
            Id = id;
            CaseId = caseId;
            VisitScheduleId = visitScheduleId;
            ExpertId = expertId;
            VisitedAt = visitedAt;
            GrowthStageId = growthStageId;
            OverallSummary = overallSummary;
            CreatedAt = DateTime.UtcNow;
        }

        public static Result<FieldVisit> Create(
            FieldVisitId id,
            CaseId caseId,
            UserId expertId,
            DateTime visitedAt,
            VisitScheduleId? visitScheduleId = null,
            LookupId? growthStageId = null,
            string? overallSummary = null)
        {
            if (id is null || caseId is null || expertId is null)
                return Result.Failure<FieldVisit>(FieldVisitErrors.NullReferenceError());

            var visitedAtValidation = ValidateVisitedAt(visitedAt);
            if (visitedAtValidation.IsFailure)
                return Result.Failure<FieldVisit>(visitedAtValidation.Error);

            var normalizedSummary = NormalizeSummary(overallSummary);
            var summaryValidation = ValidateSummaryLength(normalizedSummary);
            if (summaryValidation.IsFailure)
                return Result.Failure<FieldVisit>(summaryValidation.Error);

            var fieldVisit = new FieldVisit(
                id, caseId, visitScheduleId, expertId, visitedAt, growthStageId, normalizedSummary);

            fieldVisit.AddDomainEvent(new FieldVisitCreatedEvent(
                id.Value,
                caseId.Value,
                expertId.Value,
                visitScheduleId?.Value,
                visitedAt,
                growthStageId?.Value,
                fieldVisit.OverallSummary));

            return Result<FieldVisit>.Success(fieldVisit);
        }

        // اصلاح زمان ثبت‌شدهٔ بازدید (خطای تایپی تاریخ/ساعت) — بازدید در آینده معنا ندارد
        public Result ChangeVisitedAt(DateTime newVisitedAt)
        {
            var validation = ValidateVisitedAt(newVisitedAt);
            if (validation.IsFailure)
                return validation;

            var oldVisitedAt = VisitedAt;
            VisitedAt = newVisitedAt;
            AddDomainEvent(new FieldVisitVisitedAtChangedEvent(Id.Value, oldVisitedAt, newVisitedAt));
            return Result.Success();
        }

        // ویرایش جمع‌بندی کلی؛ ارسال null/خالی یعنی پاک‌کردن جمع‌بندی
        public Result SetOverallSummary(string? overallSummary)
        {
            var normalizedSummary = NormalizeSummary(overallSummary);
            var validation = ValidateSummaryLength(normalizedSummary);
            if (validation.IsFailure)
                return validation;

            var oldSummary = OverallSummary;
            OverallSummary = normalizedSummary;
            AddDomainEvent(new FieldVisitSummaryChangedEvent(Id.Value, oldSummary, OverallSummary));
            return Result.Success();
        }

        // ثبت/به‌روزرسانی مرحلهٔ رشد مشاهده‌شده در بازدید؛ ارسال null یعنی پاک‌کردن
        public Result SetGrowthStage(LookupId? growthStageId)
        {
            var oldGrowthStageId = GrowthStageId;
            GrowthStageId = growthStageId;
            AddDomainEvent(new FieldVisitGrowthStageChangedEvent(
                Id.Value, oldGrowthStageId?.Value, growthStageId?.Value));
            return Result.Success();
        }

        // اتصال بازدید به برنامهٔ بازدید مادر (مثلاً پس از ثبت بازدیدِ موردی و تطبیق آن با برنامه)
        // یا تغییر برنامهٔ مرتبط؛ ارسال null یعنی بازدید بدون برنامه
        public Result LinkToSchedule(VisitScheduleId? visitScheduleId)
        {
            var oldScheduleId = VisitScheduleId;
            VisitScheduleId = visitScheduleId;
            AddDomainEvent(new FieldVisitScheduleChangedEvent(
                Id.Value, oldScheduleId?.Value, visitScheduleId?.Value));
            return Result.Success();
        }

        // ثبت پاسخ یک آیتم چک‌لیست (یا پرسش آزاد) در جریان بازدید — قواعد:
        //  - منبع پرسش: آیتم قالب یا متن پرسش آزاد (آینهٔ CHECK جدول) — دقیقاً یکی
        //  - برای هر آیتم قالب فقط یک پاسخ در هر بازدید
        //  - خانهٔ مقدار: rating_id / bool_value / numeric_value / text_value — حداقل یکی
        //  - unit_id فقط با مقدار عددی معنا دارد
        //  - نوع پاسخ (answer_type_id) فیلد ماهوی است و پس از ثبت تغییر نمی‌کند
        public Result AddAnswer(
            VisitAnswerId answerId,
            ChecklistTemplateItemId? templateItemId,
            string? questionText,
            LookupId answerTypeId,
            LookupId? ratingId,
            bool? boolValue,
            decimal? numericValue,
            UnitId? unitId,
            string? textValue,
            string? note = null)
        {
            if (answerId is null || answerTypeId is null)
                return Result.Failure(FieldVisitErrors.NullReferenceError());

            if (templateItemId is null && string.IsNullOrWhiteSpace(questionText))
                return Result.Failure(FieldVisitErrors.AnswerSourceRequired());

            string? normalizedQuestionText = null;
            if (templateItemId is null)
            {
                var questionValidation = ValidateQuestionText(questionText);
                if (questionValidation.IsFailure)
                    return questionValidation;

                normalizedQuestionText = questionText!.Trim();
            }
            else if (_answers.Any(a => a.TemplateItemId?.Value == templateItemId.Value))
            {
                return Result.Failure(FieldVisitErrors.TemplateItemAlreadyAnswered(templateItemId.Value));
            }

            var valueValidation = ValidateAnswerValue(ratingId, boolValue, numericValue, unitId, textValue);
            if (valueValidation.IsFailure)
                return valueValidation;

            var noteValidation = ValidateNote(note);
            if (noteValidation.IsFailure)
                return noteValidation;

            var answer = VisitAnswer.Create(
                answerId,
                templateItemId,
                normalizedQuestionText,
                answerTypeId,
                ratingId,
                boolValue,
                numericValue,
                unitId,
                textValue,
                note);
            _answers.Add(answer);

            AddDomainEvent(new VisitAnswerAddedEvent(
                Id.Value, answerId.Value, templateItemId?.Value, answerTypeId.Value));
            return Result.Success();
        }

        // ویرایش مقدار پاسخ ثبت‌شده (اصلاح خطای ثبت) — نوع پاسخ تغییر نمی‌کند
        public Result SetAnswerValue(
            VisitAnswerId answerId,
            LookupId? ratingId,
            bool? boolValue,
            decimal? numericValue,
            UnitId? unitId,
            string? textValue)
        {
            if (answerId is null)
                return Result.Failure(FieldVisitErrors.NullReferenceError());

            var answer = FindAnswer(answerId);
            if (answer is null)
                return Result.Failure(FieldVisitErrors.AnswerNotFound(answerId.Value));

            var valueValidation = ValidateAnswerValue(ratingId, boolValue, numericValue, unitId, textValue);
            if (valueValidation.IsFailure)
                return valueValidation;

            answer.SetAnswer(ratingId, boolValue, numericValue, unitId, textValue);
            AddDomainEvent(new VisitAnswerValueChangedEvent(
                Id.Value, answerId.Value, answer.AnswerTypeId.Value));
            return Result.Success();
        }

        // ویرایش یادداشت پاسخ؛ ارسال null/خالی یعنی پاک‌کردن یادداشت
        public Result SetAnswerNote(VisitAnswerId answerId, string? note)
        {
            if (answerId is null)
                return Result.Failure(FieldVisitErrors.NullReferenceError());

            var noteValidation = ValidateNote(note);
            if (noteValidation.IsFailure)
                return noteValidation;

            var answer = FindAnswer(answerId);
            if (answer is null)
                return Result.Failure(FieldVisitErrors.AnswerNotFound(answerId.Value));

            answer.SetNote(note);
            AddDomainEvent(new VisitAnswerNoteChangedEvent(Id.Value, answerId.Value));
            return Result.Success();
        }

        // ویرایش متن پرسش آزاد — فقط برای پاسخ‌های بدون آیتم قالب؛ متن خالی پذیرفته نیست
        // (پاسخ آزاد بدون متن پرسش قاعدهٔ منبع پرسش را می‌شکند)
        public Result SetAnswerQuestion(VisitAnswerId answerId, string questionText)
        {
            if (answerId is null)
                return Result.Failure(FieldVisitErrors.NullReferenceError());

            var answer = FindAnswer(answerId);
            if (answer is null)
                return Result.Failure(FieldVisitErrors.AnswerNotFound(answerId.Value));

            if (answer.TemplateItemId is not null)
                return Result.Failure(FieldVisitErrors.QuestionTextOnlyForFreeForm());

            var questionValidation = ValidateQuestionText(questionText);
            if (questionValidation.IsFailure)
                return questionValidation;

            var oldQuestionText = answer.QuestionText;
            answer.SetQuestion(questionText!.Trim());
            AddDomainEvent(new VisitAnswerQuestionChangedEvent(
                Id.Value, answerId.Value, oldQuestionText, answer.QuestionText));
            return Result.Success();
        }

        // حذف فیزیکی پاسخ — جدول visit_answers ارجاع FK بیرونی ندارد
        // (عکس‌ها در attachments با entity_type به آن ارجاع می‌دهند، نه با FK؛
        // پاک‌سازی attachments مرتبط در لایه Application انجام می‌شود)
        public Result RemoveAnswer(VisitAnswerId answerId)
        {
            if (answerId is null)
                return Result.Failure(FieldVisitErrors.NullReferenceError());

            var answer = FindAnswer(answerId);
            if (answer is null)
                return Result.Failure(FieldVisitErrors.AnswerNotFound(answerId.Value));

            _answers.Remove(answer);
            AddDomainEvent(new VisitAnswerRemovedEvent(
                Id.Value, answerId.Value, answer.TemplateItemId?.Value));
            return Result.Success();
        }

        private VisitAnswer? FindAnswer(VisitAnswerId answerId)
            => _answers.FirstOrDefault(a => a.Id.Value == answerId.Value);

        // قاعدهٔ مشترک Create و ChangeVisitedAt
        private static Result ValidateVisitedAt(DateTime visitedAt)
        {
            if (visitedAt > DateTime.UtcNow.AddMinutes(FutureToleranceMinutes))
                return Result.Failure(FieldVisitErrors.VisitedAtInFuture(visitedAt));

            return Result.Success();
        }

        private static string? NormalizeSummary(string? overallSummary)
            => string.IsNullOrWhiteSpace(overallSummary) ? null : overallSummary.Trim();

        private static Result ValidateSummaryLength(string normalizedSummary)
        {
            if (normalizedSummary.Length > MaxSummaryLength)
                return Result.Failure(FieldVisitErrors.SummaryTooLong(normalizedSummary.Length));

            return Result.Success();
        }

        // قاعدهٔ مشترک AddAnswer و SetAnswerValue — حداقل یک خانهٔ مقدار و اعتبار عدد/واحد
        private static Result ValidateAnswerValue(
            LookupId? ratingId,
            bool? boolValue,
            decimal? numericValue,
            UnitId? unitId,
            string? textValue)
        {
            var hasAnyValue = ratingId is not null
                || boolValue is not null
                || numericValue is not null
                || !string.IsNullOrWhiteSpace(textValue);

            if (!hasAnyValue)
                return Result.Failure(FieldVisitErrors.AnswerValueRequired());

            if (unitId is not null && numericValue is null)
                return Result.Failure(FieldVisitErrors.UnitWithoutNumericValue());

            if (numericValue is not null)
            {
                if (numericValue.Value < MinNumericValue || numericValue.Value > MaxNumericValue)
                    return Result.Failure(FieldVisitErrors.NumericValueOutOfRange(numericValue.Value));

                if (decimal.Round(numericValue.Value, NumericValueScale) != numericValue.Value)
                    return Result.Failure(FieldVisitErrors.NumericValueTooPrecise(numericValue.Value));
            }

            return Result.Success();
        }

        private static Result ValidateQuestionText(string? questionText)
        {
            if (string.IsNullOrWhiteSpace(questionText))
                return Result.Failure(FieldVisitErrors.InvalidQuestionText(questionText));

            if (questionText.Trim().Length > VisitAnswer.MaxQuestionTextLength)
                return Result.Failure(FieldVisitErrors.QuestionTextTooLong(questionText.Trim()));

            return Result.Success();
        }

        private static Result ValidateNote(string? note)
        {
            if (string.IsNullOrWhiteSpace(note))
                return Result.Success();

            if (note.Trim().Length > VisitAnswer.MaxNoteLength)
                return Result.Failure(FieldVisitErrors.AnswerNoteTooLong(note.Trim().Length));

            return Result.Success();
        }
    }
}
