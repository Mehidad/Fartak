using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Payment
{
    // خطاهای تجمع پرداخت
    public static class PaymentErrors
    {
        public static readonly Error NullReference = Error.Create(
            "Payment.NullReference",
            "مقادیر اجباری پرداخت ارسال نشده است.");

        public static readonly Error CaseNotSpecified = Error.Create(
            "Payment.CaseNotSpecified",
            "برای ثبت پرداخت باید پرونده مشخص باشد.");

        public static readonly Error InvalidAmount = Error.Create(
            "Payment.InvalidAmount",
            "مبلغ پرداخت باید بزرگ‌تر از صفر باشد.");

        public static readonly Error KindNotSpecified = Error.Create(
            "Payment.KindNotSpecified",
            "نوع پرداخت باید مشخص باشد.");

        public static readonly Error StatusNotSpecified = Error.Create(
            "Payment.StatusNotSpecified",
            "وضعیت پرداخت باید مشخص باشد.");

        public static readonly Error SameStatus = Error.Create(
            "Payment.SameStatus",
            "وضعیت جدید پرداخت با وضعیت فعلی یکسان است.");

        public static readonly Error AlreadyPaid = Error.Create(
            "Payment.AlreadyPaid",
            "این پرداخت قبلاً به‌صورت قطعی ثبت شده است.");

        public static readonly Error PaidAtInFuture = Error.Create(
            "Payment.PaidAtInFuture",
            "زمان پرداخت نمی‌تواند در آینده باشد.");

        public static readonly Error CannotCancelPaidPayment = Error.Create(
            "Payment.CannotCancelPaidPayment",
            "پرداختِ ثبت‌شده قابل لغو نیست؛ برای اصلاح از رکورد برگشتی/تسویه استفاده کنید.");

        public static Error ReferenceNoTooLong(int length)
            => Error.Create(
                code: "Payment.ReferenceNoTooLong",
                message: "شماره پیگیری پرداخت نمی‌تواند بیش از ۵۰ نویسه باشد.",
                details: new Dictionary<string, object>
                {
                    ["Length"] = length,
                    ["MaxLength"] = Payment.MaxReferenceNoLength
                });

        public static readonly Error NoteTooLong = Error.Create(
            "Payment.NoteTooLong",
            "یادداشت پرداخت نمی‌تواند بیش از ۵۰۰ نویسه باشد.",
            new Dictionary<string, object> { ["MaxLength"] = Payment.MaxNoteLength });
    }
}
