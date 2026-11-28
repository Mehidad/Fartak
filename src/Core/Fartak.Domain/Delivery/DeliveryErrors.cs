using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Delivery
{
    // خطاهای تجمع تحویل — شامل خطاهای ردیف‌های تحویل (delivery_items)
    public static class DeliveryErrors
    {
        public static readonly Error NullReference = Error.Create(
            "Delivery.NullReference",
            "مقادیر اجباری تحویل ارسال نشده است.");

        public static readonly Error CaseNotSpecified = Error.Create(
            "Delivery.CaseNotSpecified",
            "برای ثبت تحویل باید پرونده مشخص باشد.");

        public static readonly Error StatusNotSpecified = Error.Create(
            "Delivery.StatusNotSpecified",
            "وضعیت تحویل باید مشخص باشد.");

        public static readonly Error SameStatus = Error.Create(
            "Delivery.SameStatus",
            "وضعیت جدید تحویل با وضعیت فعلی یکسان است.");

        public static readonly Error AlreadyDelivered = Error.Create(
            "Delivery.AlreadyDelivered",
            "زمان تحویل قبلاً ثبت شده است.");

        public static readonly Error DeliveredAtInFuture = Error.Create(
            "Delivery.DeliveredAtInFuture",
            "زمان تحویل نمی‌تواند در آینده باشد.");

        public static readonly Error NotDeliveredYet = Error.Create(
            "Delivery.NotDeliveredYet",
            "تأیید تحویل تنها پس از ثبت زمان تحویل ممکن است.");

        public static readonly Error AlreadyConfirmed = Error.Create(
            "Delivery.AlreadyConfirmed",
            "این تحویل قبلاً توسط کشاورز تأیید شده است.");

        public static readonly Error ConfirmedAtInFuture = Error.Create(
            "Delivery.ConfirmedAtInFuture",
            "زمان تأیید تحویل نمی‌تواند در آینده باشد.");

        public static readonly Error InvalidTransportCost = Error.Create(
            "Delivery.InvalidTransportCost",
            "هزینهٔ حمل نمی‌تواند منفی باشد.");

        public static Error CarrierTooLong(int length)
            => Error.Create(
                code: "Delivery.CarrierTooLong",
                message: "نام شرکت حمل نمی‌تواند بیش از ۱۲۰ نویسه باشد.",
                details: new Dictionary<string, object>
                {
                    ["Length"] = length,
                    ["MaxLength"] = Delivery.MaxCarrierLength
                });

        public static Error VehiclePlateTooLong(int length)
            => Error.Create(
                code: "Delivery.VehiclePlateTooLong",
                message: "پلاک خودرو نمی‌تواند بیش از ۲۰ نویسه باشد.",
                details: new Dictionary<string, object>
                {
                    ["Length"] = length,
                    ["MaxLength"] = Delivery.MaxVehiclePlateLength
                });

        public static readonly Error NoteTooLong = Error.Create(
            "Delivery.NoteTooLong",
            "یادداشت تحویل نمی‌تواند بیش از ۲۰۰۰ نویسه باشد.",
            new Dictionary<string, object> { ["MaxLength"] = Delivery.MaxNoteLength });

        // ─── خطاهای ردیف تحویل (delivery_items) ───

        public static readonly Error ItemNullReference = Error.Create(
            "Delivery.ItemNullReference",
            "شناسه ردیف تحویل یا مقدار ارسال نشده است.");

        public static readonly Error ItemNoReference = Error.Create(
            "Delivery.ItemNoReference",
            "هر ردیف تحویل باید به خط برنامه یا ردیف سفارش تجمیعی ارجاع داشته باشد.");

        public static readonly Error InvalidItemQuantity = Error.Create(
            "Delivery.InvalidItemQuantity",
            "مقدار ردیف تحویل باید بزرگ‌تر از صفر باشد.");

        public static Error ItemNotFound(long deliveryItemId)
            => Error.Create(
                code: "Delivery.ItemNotFound",
                message: "ردیف تحویل موردنظر یافت نشد.",
                details: new Dictionary<string, object> { ["DeliveryItemId"] = deliveryItemId });

        public static readonly Error ItemNoteTooLong = Error.Create(
            "Delivery.ItemNoteTooLong",
            "یادداشت ردیف تحویل نمی‌تواند بیش از ۲۰۰۰ نویسه باشد.",
            new Dictionary<string, object> { ["MaxLength"] = Delivery.MaxNoteLength });
    }
}
