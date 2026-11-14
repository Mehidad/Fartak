using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.FieldVisit
{

    public static class FieldVisitErrors
    {
        public static Error NullReferenceError()
            => Error.Create(
                code: "FieldVisit.NullReference",
                message: "مقادیر اجباری بازدید میدانی ارسال نشده است.");

        public static Error VisitedAtInFuture(DateTime visitedAt)
            => Error.Create(
                code: "FieldVisit.VisitedAtInFuture",
                message: "زمان انجام بازدید نمی‌تواند در آینده باشد.",
                details: new Dictionary<string, object>
                {
                    ["VisitedAt"] = visitedAt
                });

        public static Error SummaryTooLong(int length)
            => Error.Create(
                code: "FieldVisit.SummaryTooLong",
                message: "جمع‌بندی کلی بازدید نباید بیش از ۴۰۰۰ کاراکتر باشد.",
                details: new Dictionary<string, object>
                {
                    ["Length"] = length
                });

        public static Error AnswerNotFound(long answerId)
            => Error.Create(
                code: "FieldVisit.AnswerNotFound",
                message: "پاسخ موردنظر در این بازدید یافت نشد.",
                details: new Dictionary<string, object>
                {
                    ["AnswerId"] = answerId
                });

        public static Error AnswerSourceRequired()
            => Error.Create(
                code: "FieldVisit.AnswerSourceRequired",
                message: "هر پاسخ باید یا به آیتم قالب چک‌لیست متصل باشد یا متن پرسش آزاد داشته باشد.");

        public static Error InvalidQuestionText(string? questionText)
            => Error.Create(
                code: "FieldVisit.InvalidQuestionText",
                message: "متن پرسش آزاد الزامی است.",
                details: new Dictionary<string, object>
                {
                    ["QuestionText"] = questionText ?? string.Empty
                });

        public static Error QuestionTextTooLong(string questionText)
            => Error.Create(
                code: "FieldVisit.QuestionTextTooLong",
                message: "متن پرسش آزاد نباید بیش از ۲۰۰ کاراکتر باشد.",
                details: new Dictionary<string, object>
                {
                    ["QuestionText"] = questionText
                });

        public static Error QuestionTextOnlyForFreeForm()
            => Error.Create(
                code: "FieldVisit.QuestionTextOnlyForFreeForm",
                message: "متن پرسش فقط برای پاسخ‌های آزاد (بدون آیتم قالب) قابل ویرایش است.");

        public static Error TemplateItemAlreadyAnswered(long templateItemId)
            => Error.Create(
                code: "FieldVisit.TemplateItemAlreadyAnswered",
                message: "برای هر آیتم قالب چک‌لیست در هر بازدید فقط یک پاسخ قابل ثبت است.",
                details: new Dictionary<string, object>
                {
                    ["TemplateItemId"] = templateItemId
                });

        public static Error AnswerValueRequired()
            => Error.Create(
                code: "FieldVisit.AnswerValueRequired",
                message: "حداقل یکی از خانه‌های مقدار پاسخ (امتیاز، بله/خیر، عدد، متن) باید پر شود.");

        public static Error UnitWithoutNumericValue()
            => Error.Create(
                code: "FieldVisit.UnitWithoutNumericValue",
                message: "واحد اندازه‌گیری فقط برای پاسخ عددی معنا دارد.");

        public static Error NumericValueOutOfRange(decimal value)
            => Error.Create(
                code: "FieldVisit.NumericValueOutOfRange",
                message: "مقدار عددی پاسخ خارج از محدودهٔ مجاز است.",
                details: new Dictionary<string, object>
                {
                    ["Value"] = value,
                    ["Min"] = FieldVisit.MinNumericValue,
                    ["Max"] = FieldVisit.MaxNumericValue
                });

        public static Error NumericValueTooPrecise(decimal value)
            => Error.Create(
                code: "FieldVisit.NumericValueTooPrecise",
                message: "مقدار عددی پاسخ حداکثر ۳ رقم اعشار می‌پذیرد.",
                details: new Dictionary<string, object>
                {
                    ["Value"] = value
                });

        public static Error AnswerNoteTooLong(int length)
            => Error.Create(
                code: "FieldVisit.AnswerNoteTooLong",
                message: "یادداشت پاسخ نباید بیش از ۱۰۰۰ کاراکتر باشد.",
                details: new Dictionary<string, object>
                {
                    ["Length"] = length
                });
    }
}
