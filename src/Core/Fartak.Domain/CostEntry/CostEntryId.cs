using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.CostEntry
{
    // شناسه یک ردیف از جدول cost_entries (ردیف‌های هزینهٔ واقعی پرونده)
    public sealed record CostEntryId
    {
        public long Value { get; }

        private CostEntryId(long value) => Value = value;

        public static CostEntryId From(long value)
        {
            if (value <= 0)
                throw new DomainValidationException(ErrorCodes.InvalidId, "CostEntryId must be positive.");
            return new CostEntryId(value);
        }

        public static CostEntryId Empty => new(0);

        public override string ToString() => Value.ToString();
    }
}
