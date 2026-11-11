using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.FarmerProfile
{
    // کد ملی ایرانی — ۱۰ رقم با رقم کنترل (ضریب ۱۰ تا ۲ و باقیمانده ۱۱)
    // مطابق ERD اختیاری است؛ اما اگر ارسال شود باید از نظر ساختار و رقم کنترل کاملاً معتبر باشد
    public sealed class NationalId : ValueObject
    {
        public const int Length = 10;

        public string Value { get; }

        private NationalId() { } // برای EF Core

        private NationalId(string value)
        {
            Value = value;
        }

        public static NationalId Create(string value)
        {
            var digits = value?.Trim() ?? string.Empty;

            if (digits.Length != Length || !digits.All(char.IsDigit))
                throw new DomainValidationException(
                    "FarmerProfile.InvalidNationalId",
                    "کد ملی باید دقیقاً ۱۰ رقم باشد.",
                    new Dictionary<string, object> { ["NationalId"] = digits });

            // رشته‌هایی مثل «۱۱۱۱۱۱۱۱۱۱» از نظر الگوریتم پاس می‌شوند ولی در عمل نامعتبرند
            if (digits.Distinct().Count() == 1)
                throw new DomainValidationException(
                    "FarmerProfile.InvalidNationalId",
                    "کد ملی واردشده معتبر نیست.",
                    new Dictionary<string, object> { ["NationalId"] = digits });

            if (!HasValidCheckDigit(digits))
                throw new DomainValidationException(
                    "FarmerProfile.InvalidNationalId",
                    "رقم کنترل کد ملی معتبر نیست.",
                    new Dictionary<string, object> { ["NationalId"] = digits });

            return new NationalId(digits);
        }

        // الگوریتم رسمی رقم کنترل کد ملی — راستی‌آزمایی‌شده با نمونه معتبر «0079241506»
        private static bool HasValidCheckDigit(string digits)
        {
            var sum = 0;
            for (var i = 0; i < 9; i++)
                sum += (digits[i] - '0') * (10 - i);

            var remainder = sum % 11;
            var control = digits[9] - '0';

            return remainder < 2
                ? control == remainder
                : control == 11 - remainder;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;
    }
}
