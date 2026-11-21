using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.PlanVersion
{
    // شناسه یک ردیف از جدول plan_lines (خطوط برنامه: قلم/اقدام) — فرزند Aggregate «PlanVersion»
    public sealed record PlanLineId
    {
        public long Value { get; }

        private PlanLineId(long value) => Value = value;

        public static PlanLineId From(long value)
        {
            if (value <= 0)
                throw new DomainValidationException(ErrorCodes.InvalidId, "PlanLineId must be positive.");
            return new PlanLineId(value);
        }

        public static PlanLineId Empty => new(0);

        public override string ToString() => Value.ToString();
    }
}
