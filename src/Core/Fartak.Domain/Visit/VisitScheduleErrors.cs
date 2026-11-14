using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Visit
{
    public static class VisitScheduleErrors
    {
        public static Error NullReferenceError()
            => Error.Create(
                code: "VisitSchedule.NullReference",
                message: "مقادیر اجباری زمان‌بندی بازدید ارسال نشده است.");

        public static Error ScheduledForInPast(DateTime scheduledFor)
            => Error.Create(
                code: "VisitSchedule.ScheduledForInPast",
                message: "زمان جدید بازدید نمی‌تواند در گذشته باشد.",
                details: new Dictionary<string, object>
                {
                    ["ScheduledFor"] = scheduledFor
                });

        public static Error ScheduledForUnchanged()
            => Error.Create(
                code: "VisitSchedule.ScheduledForUnchanged",
                message: "زمان جدید بازدید همان زمان فعلی است.");

        public static Error StatusUnchanged()
            => Error.Create(
                code: "VisitSchedule.StatusUnchanged",
                message: "وضعیت ارسالی همان وضعیت فعلی زمان‌بندی بازدید است.");

        public static Error ChecklistTemplateUnchanged()
            => Error.Create(
                code: "VisitSchedule.ChecklistTemplateUnchanged",
                message: "قالب چک‌لیست ارسالی همان قالب فعلی زمان‌بندی بازدید است.");

        public static Error NoteTooLong(string note)
            => Error.Create(
                code: "VisitSchedule.NoteTooLong",
                message: "یادداشت زمان‌بندی بازدید نباید بیش از ۱۰۰۰ کاراکتر باشد.",
                details: new Dictionary<string, object>
                {
                    ["Note"] = note
                });

        public static Error NoteUnchanged()
            => Error.Create(
                code: "VisitSchedule.NoteUnchanged",
                message: "یادداشت ارسالی همان یادداشت فعلی زمان‌بندی بازدید است.");
    }
}
