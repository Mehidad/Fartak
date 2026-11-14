using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Checklist;
using Fartak.Domain.Common;
using Fartak.Domain.Framework;
using Fartak.Domain.Unit;

namespace Fartak.Domain.FieldVisit
{
    // پاسخ یک آیتم چک‌لیست (یا پرسش آزاد) در بازدید میدانی — موجودیت فرزندِ FieldVisit (جدول visit_answers)
    // فقط از طریق خود Aggregate ساخته می‌شود: FieldVisit.AddAnswer
    // جدول visit_answers ستون timestamp ندارد → بدون CreatedAt/UpdatedAt.
    public sealed class VisitAnswer : Entity<VisitAnswerId>
    {
        // ستون question_text در DB از نوع varchar(200) است
        public const int MaxQuestionTextLength = 200;
        // سقف عملیاتی یادداشت پاسخ (ستون note از نوع text است و در DB سقف ندارد)
        public const int MaxNoteLength = 1000;

        public ChecklistTemplateItemId? TemplateItemId { get; private set; }  // آیتم قالب چک‌لیست (NULL = پرسش آزاد)
        public string? QuestionText { get; private set; }                     // متن پرسش آزاد (فقط برای پاسخ آزاد)
        public LookupId AnswerTypeId { get; private set; }                    // نوع پاسخ (دامنه answer_type) — تغییرناپذیر
        public LookupId? RatingId { get; private set; }                       // امتیاز کیفی (دامنه rating)
        public bool? BoolValue { get; private set; }
        public decimal? NumericValue { get; private set; }                    // هم‌خوان numeric(12,3)
        public UnitId? UnitId { get; private set; }                           // واحد مقدار عددی
        public string? TextValue { get; private set; }
        public string? Note { get; private set; }

        private VisitAnswer() { } // برای EF Core

        private VisitAnswer(
            VisitAnswerId id,
            ChecklistTemplateItemId? templateItemId,
            string? questionText,
            LookupId answerTypeId,
            LookupId? ratingId,
            bool? boolValue,
            decimal? numericValue,
            UnitId? unitId,
            string? textValue,
            string? note)
        {
            Id = id;
            TemplateItemId = templateItemId;
            QuestionText = questionText;
            AnswerTypeId = answerTypeId;
            RatingId = ratingId;
            BoolValue = boolValue;
            NumericValue = numericValue;
            UnitId = unitId;
            TextValue = textValue;
            Note = note;
        }

        // فقط ریشهٔ تجمع صدا می‌زند؛ ورودی‌ها پیش‌اعتبارسنجی شده‌اند و اینجا فقط دفاع آخر است
        internal static VisitAnswer Create(
            VisitAnswerId id,
            ChecklistTemplateItemId? templateItemId,
            string? questionText,
            LookupId answerTypeId,
            LookupId? ratingId,
            bool? boolValue,
            decimal? numericValue,
            UnitId? unitId,
            string? textValue,
            string? note)
        {
            if (id is null || answerTypeId is null)
                throw new DomainValidationException(
                    "FieldVisit.AnswerInvalid",
                    "شناسه پاسخ یا نوع پاسخ ارسال نشده است.");

            if (templateItemId is null && string.IsNullOrWhiteSpace(questionText))
                throw new DomainValidationException(
                    "FieldVisit.AnswerSourceInvalid",
                    "پاسخ باید یا به آیتم قالب چک‌لیست متصل باشد یا متن پرسش آزاد داشته باشد.");

            return new VisitAnswer(
                id, templateItemId, questionText, answerTypeId,
                ratingId, boolValue, numericValue, unitId, textValue, note);
        }

        // ویرایش مقدار پاسخ — نوع پاسخ (AnswerTypeId) پس از ثبت تغییر نمی‌کند
        internal void SetAnswer(
            LookupId? ratingId,
            bool? boolValue,
            decimal? numericValue,
            UnitId? unitId,
            string? textValue)
        {
            RatingId = ratingId;
            BoolValue = boolValue;
            NumericValue = numericValue;
            UnitId = unitId;
            TextValue = textValue;
        }

        internal void SetNote(string? note) => Note = note;

        internal void SetQuestion(string questionText) => QuestionText = questionText;
    }
}
