using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Case
{
    // درصد کارمزد اختصاصی پرونده — معادل ستون commission_rate numeric(5,2)
    // مقدار تهی (null) در Case یعنی «کارمزد پیش‌فرض تنظیمات سیستم» اعمال شود.
    public sealed class CommissionRate : ValueObject
    {
        public const decimal MinRate = 0m;
        public const decimal MaxRate = 100m;

        public decimal Rate { get; }

        private CommissionRate() { } // برای EF Core

        private CommissionRate(decimal rate) => Rate = rate;

        public static CommissionRate Create(decimal rate)
        {
            var rounded = Math.Round(rate, 2);

            if (rounded < MinRate || rounded > MaxRate)
                throw new DomainValidationException(
                    "Case.InvalidCommissionRate",
                    "درصد کارمزد باید بین ۰ و ۱۰۰ باشد.",
                    new Dictionary<string, object> { ["Rate"] = rate });

            return new CommissionRate(rounded);
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Rate;
        }

        public override string ToString() => $"{Rate}%";
    }
}
