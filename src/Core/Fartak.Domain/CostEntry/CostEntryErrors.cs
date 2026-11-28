using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.CostEntry
{
    // خطاهای تجمع ردیف هزینه
    public static class CostEntryErrors
    {
        public static readonly Error NullReference = Error.Create(
            "CostEntry.NullReference",
            "شناسهٔ ردیف هزینه ارسال نشده است.");

        public static readonly Error CaseNotSpecified = Error.Create(
            "CostEntry.CaseNotSpecified",
            "برای ثبت هزینه باید پرونده مشخص باشد.");

        public static readonly Error CategoryNotSpecified = Error.Create(
            "CostEntry.CategoryNotSpecified",
            "سرفصل هزینه باید مشخص باشد.");

        public static readonly Error InvalidAmount = Error.Create(
            "CostEntry.InvalidAmount",
            "مبلغ هزینه نمی‌تواند منفی باشد.");

        public static readonly Error OccurredAtInFuture = Error.Create(
            "CostEntry.OccurredAtInFuture",
            "تاریخ هزینه نمی‌تواند در آینده باشد.");

        public static Error DescriptionTooLong()
            => Error.Create(
                code: "CostEntry.DescriptionTooLong",
                message: "شرح هزینه نمی‌تواند بیش از ۲۰۰۰ نویسه باشد.",
                details: new Dictionary<string, object>
                {
                    ["MaxLength"] = CostEntry.MaxDescriptionLength
                });

        public static readonly Error DescriptionUnchanged = Error.Create(
            "CostEntry.DescriptionUnchanged",
            "شرح جدید هزینه با شرح فعلی یکسان است.");

        public static readonly Error SourceUnchanged = Error.Create(
            "CostEntry.SourceUnchanged",
            "منبع هزینهٔ جدید با منبع فعلی یکسان است.");

        public static readonly Error AlreadyLinkedToPlanLine = Error.Create(
            "CostEntry.AlreadyLinkedToPlanLine",
            "این هزینه قبلاً به یک خط برنامه پیوند خورده است؛ ابتدا پیوند را بردارید.");

        public static readonly Error PlanLineNotLinked = Error.Create(
            "CostEntry.PlanLineNotLinked",
            "این هزینه به هیچ خط برنامه‌ای پیوند نداشته است.");
    }
}
