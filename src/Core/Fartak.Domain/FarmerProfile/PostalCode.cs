using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.FarmerProfile
{
    // کد پستی ایران — دقیقاً ۱۰ رقم
    public sealed class PostalCode : ValueObject
    {
        public const int Length = 10;

        public string Value { get; }

        private PostalCode() { } // برای EF Core

        private PostalCode(string value)
        {
            Value = value;
        }

        public static PostalCode Create(string value)
        {
            var digits = value?.Trim() ?? string.Empty;

            if (digits.Length != Length || !digits.All(char.IsDigit))
                throw new DomainValidationException(
                    "FarmerProfile.InvalidPostalCode",
                    "کد پستی باید دقیقاً ۱۰ رقم باشد.",
                    new Dictionary<string, object> { ["PostalCode"] = digits });

            return new PostalCode(digits);
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;
    }
}
