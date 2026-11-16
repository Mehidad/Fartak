using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.VisitSchedule
{
    public static class VisitScheduleErrors
    {
        public static Error NullReferenceError()
            => Error.Create(
                code: "VisitSchedule.NullReference",
                message: "مقادیر اجباری زمان‌بندی بازدید ارسال نشده است.");

        public static Error UnknownStatusCode(string statusCode)
            => Error.Create(
                code: "VisitSchedule.UnknownStatusCode",
                message: "کد وضعیت برای دامنه visit_status شناخته‌شده نیست؛ کدهای مجاز: planned، done، canceled، expired.",
                details: new Dictionary<string, object>
                {
                    ["StatusCode"] = statusCode
                });

        public static Error InvalidInitialStatus(string statusCode)
            => Error.Create(
                code: "VisitSchedule.InvalidInitialStatus",
                message: "زمان‌بندی جدید فقط با وضعیت «برنامه‌ریزی‌شده» (planned) ساخته می‌شود.",
                details: new Dictionary<string, object>
                {
                    ["InitialStatusCode"] = statusCode
                });

        public static Error ScheduledForInPast(DateTime scheduledFor)
            => Error.Create(
                code: "VisitSchedule.ScheduledForInPast",
                message: "زمان بازدید نمی‌تواند در گذشته باشد.",
                details: new Dictionary<string, object>
                {
                    ["ScheduledFor"] = scheduledFor.ToString("yyyy-MM-dd HH:mm:ss"),
                    ["ToleranceMinutes"] = 5
                });

        public static Error NoteTooLong(int length)
            => Error.Create(
                code: "VisitSchedule.NoteTooLong",
                message: "متن یادداشت زمان‌بندی نباید بیش از ۲٬۰۰۰ کاراکتر باشد.",
                details: new Dictionary<string, object>
                {
                    ["Length"] = length
                });

        public static Error ScheduleNotOpen(string currentStatusCode)
            => Error.Create(
                code: "VisitSchedule.ScheduleNotOpen",
                message: "فقط زمان‌بندیِ در وضعیت «برنامه‌ریزی‌شده» قابل ویرایش است؛ این رکورد بسته شده است.",
                details: new Dictionary<string, object>
                {
                    ["CurrentStatusCode"] = currentStatusCode
                });

        public static Error StatusUnchanged(long visitScheduleId, long statusId)
            => Error.Create(
                code: "VisitSchedule.StatusUnchanged",
                message: "زمان‌بندی از قبل در همین وضعیت قرار دارد.",
                details: new Dictionary<string, object>
                {
                    ["VisitScheduleId"] = visitScheduleId,
                    ["StatusId"] = statusId
                });

        public static Error InvalidStatusTransition(string fromStatusCode, string toStatusCode)
            => Error.Create(
                code: "VisitSchedule.InvalidStatusTransition",
                message: "گذار وضعیت زمان‌بندی از وضعیت فعلی به وضعیت مقصد مجاز نیست؛ رکوردهای بسته (done/canceled/expired) دیگر تغییر وضعیت نمی‌پذیرند.",
                details: new Dictionary<string, object>
                {
                    ["FromStatusCode"] = fromStatusCode,
                    ["ToStatusCode"] = toStatusCode
                });

        public static Error ScheduledForUnchanged(DateTime scheduledFor)
            => Error.Create(
                code: "VisitSchedule.ScheduledForUnchanged",
                message: "زمان جدید با زمان فعلی بازدید یکسان است.",
                details: new Dictionary<string, object>
                {
                    ["ScheduledFor"] = scheduledFor.ToString("yyyy-MM-dd HH:mm:ss")
                });

        public static Error VisitTypeUnchanged(long visitTypeId)
            => Error.Create(
                code: "VisitSchedule.VisitTypeUnchanged",
                message: "نوع بازدید از قبل همین است.",
                details: new Dictionary<string, object>
                {
                    ["VisitTypeId"] = visitTypeId
                });

        public static Error ChecklistTemplateUnchanged(long? checklistTemplateId)
            => Error.Create(
                code: "VisitSchedule.ChecklistTemplateUnchanged",
                message: "قالب چک‌لیست زمان‌بندی از قبل همین است.",
                details: new Dictionary<string, object>
                {
                    ["ChecklistTemplateId"] = checklistTemplateId ?? 0
                });

        public static Error NoteUnchanged()
            => Error.Create(
                code: "VisitSchedule.NoteUnchanged",
                message: "یادداشت زمان‌بندی از قبل همین است.");
    }
}
