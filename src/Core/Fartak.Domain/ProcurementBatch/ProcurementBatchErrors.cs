using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.ProcurementBatch
{
    // خطاهای تجمع سفارش تجمیعی — شامل خطاهای خطوط سفارش (batch_lines)
    public static class ProcurementBatchErrors
    {
        public static readonly Error NullReference = Error.Create(
            "ProcurementBatch.NullReference",
            "مقادیر اجباری سفارش تجمیعی ارسال نشده است.");

        public static readonly Error BatchCodeNotSpecified = Error.Create(
            "ProcurementBatch.BatchCodeNotSpecified",
            "کد سفارش تجمیعی باید مشخص باشد.");

        public static Error BatchCodeTooLong(int length)
            => Error.Create(
                code: "ProcurementBatch.BatchCodeTooLong",
                message: "کد سفارش تجمیعی نمی‌تواند بیش از ۳۰ نویسه باشد.",
                details: new Dictionary<string, object>
                {
                    ["Length"] = length,
                    ["MaxLength"] = ProcurementBatch.MaxBatchCodeLength
                });

        public static readonly Error StatusNotSpecified = Error.Create(
            "ProcurementBatch.StatusNotSpecified",
            "وضعیت سفارش تجمیعی باید مشخص باشد.");

        public static readonly Error SameStatus = Error.Create(
            "ProcurementBatch.SameStatus",
            "وضعیت جدید سفارش با وضعیت فعلی یکسان است.");

        public static Error NoteTooLong()
            => Error.Create(
                code: "ProcurementBatch.NoteTooLong",
                message: "یادداشت سفارش تجمیعی نمی‌تواند بیش از ۲۰۰۰ نویسه باشد.",
                details: new Dictionary<string, object> { ["MaxLength"] = ProcurementBatch.MaxNoteLength });

        // ─── خطاهای خط سفارش (batch_lines) ───

        public static readonly Error LineNullReference = Error.Create(
            "ProcurementBatch.LineNullReference",
            "شناسه خط برنامه یا مقدار ردیف سفارش ارسال نشده است.");

        public static readonly Error InvalidQuantity = Error.Create(
            "ProcurementBatch.InvalidQuantity",
            "مقدار ردیف سفارش باید بزرگ‌تر از صفر باشد.");

        public static Error LineDuplicated(long planLineId)
            => Error.Create(
                code: "ProcurementBatch.LineDuplicated",
                message: "این خط برنامه قبلاً به همین سفارش تجمیعی تخصیص یافته است.",
                details: new Dictionary<string, object> { ["PlanLineId"] = planLineId });

        public static Error LineNotFound(long batchLineId)
            => Error.Create(
                code: "ProcurementBatch.LineNotFound",
                message: "ردیف سفارش موردنظر در این سفارش یافت نشد.",
                details: new Dictionary<string, object> { ["BatchLineId"] = batchLineId });

        public static Error LineNoteTooLong()
            => Error.Create(
                code: "ProcurementBatch.LineNoteTooLong",
                message: "یادداشت ردیف سفارش نمی‌تواند بیش از ۲۰۰۰ نویسه باشد.",
                details: new Dictionary<string, object> { ["MaxLength"] = ProcurementBatch.MaxNoteLength });
    }
}
