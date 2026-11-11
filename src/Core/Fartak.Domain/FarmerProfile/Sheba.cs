using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.FarmerProfile
{
    // شماره شبا (IBAN ایران) — ۲۶ کاراکتر: «IR» + ۲۴ رقم (۲ رقم کنترل + ۲۲ رقم حساب)
    // اعتبارسنجی با الگوریتم استاندارد mod-97 (ISO 13616)
    public sealed class Sheba : ValueObject
    {
        public const int TotalLength = 26;
        private const string CountryCode = "IR";

        public string Value { get; }

        private Sheba() { } // برای EF Core

        private Sheba(string value)
        {
            Value = value;
        }

        public static Sheba Create(string value)
        {
            // نرمال‌سازی: حذف فاصله‌ها و حروف بزرگ — نوشتن شبا با فاصله در فرم‌ها رایج است
            var normalized = string.Concat(
                (value ?? string.Empty).Where(c => !char.IsWhiteSpace(c)))
                .ToUpperInvariant();

            if (normalized.Length != TotalLength || !normalized.StartsWith(CountryCode, StringComparison.Ordinal))
                throw new DomainValidationException(
                    "FarmerProfile.InvalidSheba",
                    "شبا باید ۲۶ کاراکتر باشد و با «IR» شروع شود.",
                    new Dictionary<string, object> { ["Sheba"] = value ?? string.Empty });

            if (!normalized.Substring(2).All(char.IsDigit))
                throw new DomainValidationException(
                    "FarmerProfile.InvalidSheba",
                    "بخش عددی شبا باید فقط رقم باشد.",
                    new Dictionary<string, object> { ["Sheba"] = value ?? string.Empty });

            if (!HasValidChecksum(normalized))
                throw new DomainValidationException(
                    "FarmerProfile.InvalidSheba",
                    "رقم کنترل شبا معتبر نیست.",
                    new Dictionary<string, object> { ["Sheba"] = value ?? string.Empty });

            return new Sheba(normalized);
        }

        // الگوریتم استاندارد ISO 13616 — راستی‌آزمایی‌شده با نمونه معتبر «IR580540105180021273113007»
        private static bool HasValidChecksum(string value)
        {
            // چهار کاراکتر اول (کد کشور + رقم کنترل) به انتها منتقل می‌شوند
            var rearranged = value.Substring(4) + value.Substring(0, 4);
            var remainder = 0;

            foreach (var ch in rearranged)
            {
                if (ch >= '0' && ch <= '9')
                {
                    remainder = (remainder * 10 + (ch - '0')) % 97;
                }
                else
                {
                    // هر حرف با مقدار دو رقمی جایگزین می‌شود (A=10، ...، Z=35)؛ I=18 و R=27
                    var letterValue = (char.ToUpperInvariant(ch) - 'A') + 10;
                    remainder = (remainder * 100 + letterValue) % 97;
                }
            }

            return remainder == 1;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;
    }
}
