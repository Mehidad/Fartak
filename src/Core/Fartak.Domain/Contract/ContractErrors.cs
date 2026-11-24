using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Contract
{
    // خطاهای تجمع قرارداد
    public static class ContractErrors
    {
        public static readonly Error NullReference = Error.Create(
            "Contract.NullReference",
            "مقادیر اجباری قرارداد ارسال نشده است.");

        public static readonly Error CaseNotSpecified = Error.Create(
            "Contract.CaseNotSpecified",
            "برای عقد قرارداد باید پرونده مشخص باشد.");

        public static readonly Error FarmerNotSpecified = Error.Create(
            "Contract.FarmerNotSpecified",
            "طرف قرارداد (کشاورز) باید مشخص باشد.");

        public static readonly Error StatusNotSpecified = Error.Create(
            "Contract.StatusNotSpecified",
            "وضعیت قرارداد باید مشخص باشد.");

        public static readonly Error TitleNotSpecified = Error.Create(
            "Contract.TitleNotSpecified",
            "عنوان قرارداد نمی‌تواند خالی باشد.");

        public static Error TitleTooLong(int length)
            => Error.Create(
                code: "Contract.TitleTooLong",
                message: "عنوان قرارداد نمی‌تواند بیش از ۲۰۰ نویسه باشد.",
                details: new Dictionary<string, object>
                {
                    ["Length"] = length,
                    ["MaxLength"] = Contract.MaxTitleLength
                });

        public static readonly Error InvalidTotalAmount = Error.Create(
            "Contract.InvalidTotalAmount",
            "مبلغ کل قرارداد نمی‌تواند منفی باشد.");

        public static readonly Error InvalidDateRange = Error.Create(
            "Contract.InvalidDateRange",
            "تاریخ پایان قرارداد نمی‌تواند قبل از تاریخ شروع باشد.");

        public static readonly Error SameStatus = Error.Create(
            "Contract.SameStatus",
            "وضعیت جدید قرارداد با وضعیت فعلی یکسان است.");

        public static readonly Error BankGuaranteeAlreadySet = Error.Create(
            "Contract.BankGuaranteeAlreadySet",
            "ضمانت‌نامه بانکی قبلاً برای این قرارداد ثبت شده است؛ ابتدا آن را پاک کنید.");

        public static readonly Error BankGuaranteeNotSet = Error.Create(
            "Contract.BankGuaranteeNotSet",
            "ضمانت‌نامه بانکی برای این قرارداد ثبت نشده است.");

        public static readonly Error BankNameNotSpecified = Error.Create(
            "Contract.BankNameNotSpecified",
            "نام بانک صادرکننده ضمانت‌نامه نمی‌تواند خالی باشد.");

        public static Error BankNameTooLong(int length)
            => Error.Create(
                code: "Contract.BankNameTooLong",
                message: "نام بانک نمی‌تواند بیش از ۱۰۰ نویسه باشد.",
                details: new Dictionary<string, object>
                {
                    ["Length"] = length,
                    ["MaxLength"] = Contract.MaxBankNameLength
                });

        public static readonly Error GuaranteeNumberNotSpecified = Error.Create(
            "Contract.GuaranteeNumberNotSpecified",
            "شماره ضمانت‌نامه بانکی نمی‌تواند خالی باشد.");

        public static Error GuaranteeNumberTooLong(int length)
            => Error.Create(
                code: "Contract.GuaranteeNumberTooLong",
                message: "شماره ضمانت‌نامه نمی‌تواند بیش از ۵۰ نویسه باشد.",
                details: new Dictionary<string, object>
                {
                    ["Length"] = length,
                    ["MaxLength"] = Contract.MaxGuaranteeNumberLength
                });

        public static readonly Error InvalidGuaranteeAmount = Error.Create(
            "Contract.InvalidGuaranteeAmount",
            "مبلغ ضمانت‌نامه بانکی باید بزرگ‌تر از صفر باشد.");

        public static readonly Error NoteTooLong = Error.Create(
            "Contract.NoteTooLong",
            "یادداشت قرارداد نمی‌تواند بیش از ۵۰۰ نویسه باشد.",
            new Dictionary<string, object> { ["MaxLength"] = Contract.MaxNoteLength });
    }
}
