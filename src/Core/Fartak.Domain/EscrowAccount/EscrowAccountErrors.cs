using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.EscrowAccount
{
    // خطاهای تجمع حساب امانی
    public static class EscrowAccountErrors
    {
        public static readonly Error NullReference = Error.Create(
            "EscrowAccount.NullReference",
            "مقادیر اجباری حساب امانی ارسال نشده است.");

        public static readonly Error CaseNotSpecified = Error.Create(
            "EscrowAccount.CaseNotSpecified",
            "برای افتتاح حساب امانی باید پرونده مشخص باشد.");

        public static readonly Error FarmerNotSpecified = Error.Create(
            "EscrowAccount.FarmerNotSpecified",
            "کشاورز صاحب حساب امانی باید مشخص باشد.");

        public static readonly Error InvalidHeldAmount = Error.Create(
            "EscrowAccount.InvalidHeldAmount",
            "مبلغ نگه‌داشته‌شده در حساب امانی باید بزرگ‌تر از صفر باشد.");

        public static readonly Error CommissionRateNotSpecified = Error.Create(
            "EscrowAccount.CommissionRateNotSpecified",
            "درصد کارمزد باید مشخص باشد.");

        public static readonly Error InvalidCommissionRate = Error.Create(
            "EscrowAccount.InvalidCommissionRate",
            "محاسبهٔ کارمزد و سهم پرداختی نامعتبر است.");

        public static readonly Error StatusNotSpecified = Error.Create(
            "EscrowAccount.StatusNotSpecified",
            "وضعیت حساب امانی باید مشخص باشد.");

        public static readonly Error AlreadyReleased = Error.Create(
            "EscrowAccount.AlreadyReleased",
            "مبلغ این حساب امانی قبلاً آزاد شده است.");

        public static readonly Error AlreadyRefunded = Error.Create(
            "EscrowAccount.AlreadyRefunded",
            "مبلغ این حساب امانی قبلاً برگردانده شده است.");
    }
}
