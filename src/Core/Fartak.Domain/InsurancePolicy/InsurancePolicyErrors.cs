using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.InsurancePolicy
{
    // خطاهای تجمع بیمه‌نامه
    public static class InsurancePolicyErrors
    {
        public static readonly Error NullReference = Error.Create(
            "InsurancePolicy.NullReference",
            "مقادیر اجباری بیمه‌نامه ارسال نشده است.");

        public static readonly Error CaseNotSpecified = Error.Create(
            "InsurancePolicy.CaseNotSpecified",
            "برای ثبت بیمه‌نامه باید پرونده مشخص باشد.");

        public static readonly Error CompanyNameNotSpecified = Error.Create(
            "InsurancePolicy.CompanyNameNotSpecified",
            "نام شرکت بیمه باید مشخص باشد.");

        public static Error CompanyNameTooLong(int length)
            => Error.Create(
                code: "InsurancePolicy.CompanyNameTooLong",
                message: "نام شرکت بیمه نمی‌تواند بیش از ۱۲۰ نویسه باشد.",
                details: new Dictionary<string, object>
                {
                    ["Length"] = length,
                    ["MaxLength"] = InsurancePolicy.MaxCompanyNameLength
                });

        public static readonly Error PolicyNoNotSpecified = Error.Create(
            "InsurancePolicy.PolicyNoNotSpecified",
            "شماره بیمه‌نامه باید مشخص باشد.");

        public static Error PolicyNoTooLong(int length)
            => Error.Create(
                code: "InsurancePolicy.PolicyNoTooLong",
                message: "شماره بیمه‌نامه نمی‌تواند بیش از ۶۰ نویسه باشد.",
                details: new Dictionary<string, object>
                {
                    ["Length"] = length,
                    ["MaxLength"] = InsurancePolicy.MaxPolicyNoLength
                });

        public static readonly Error InvalidCoverageRange = Error.Create(
            "InsurancePolicy.InvalidCoverageRange",
            "تاریخ پایان پوشش بیمه‌نامه نمی‌تواند قبل از تاریخ شروع باشد.");

        public static readonly Error InvalidPremium = Error.Create(
            "InsurancePolicy.InvalidPremium",
            "حق بیمه نمی‌تواند منفی باشد.");

        public static readonly Error StatusNotSpecified = Error.Create(
            "InsurancePolicy.StatusNotSpecified",
            "وضعیت بیمه‌نامه باید مشخص باشد.");

        public static readonly Error SameStatus = Error.Create(
            "InsurancePolicy.SameStatus",
            "وضعیت جدید بیمه‌نامه با وضعیت فعلی یکسان است.");

        public static readonly Error MetaUnchanged = Error.Create(
            "InsurancePolicy.MetaUnchanged",
            "مقدار جدید Meta با مقدار فعلی یکسان است.");
    }
}
