using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.CostCategory
{
    // شناسه یک ردیف از جدول cost_categories (سرفصل‌های هزینه)
    public sealed record CostCategoryId
    {
        public long Value { get; }

        private CostCategoryId(long value) => Value = value;

        public static CostCategoryId From(long value)
        {
            if (value <= 0)
                throw new DomainValidationException(ErrorCodes.InvalidId, "CostCategoryId must be positive.");
            return new CostCategoryId(value);
        }

        public static CostCategoryId Empty => new(0);

        public override string ToString() => Value.ToString();
    }
}
