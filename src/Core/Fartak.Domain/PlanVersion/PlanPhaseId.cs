using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.PlanVersion
{
    // شناسه یک ردیف از جدول plan_phases (فازهای زمان‌بندی برنامه) — فرزند Aggregate «PlanVersion»
    public sealed record PlanPhaseId
    {
        public long Value { get; }

        private PlanPhaseId(long value) => Value = value;

        public static PlanPhaseId From(long value)
        {
            if (value <= 0)
                throw new DomainValidationException(ErrorCodes.InvalidId, "PlanPhaseId must be positive.");
            return new PlanPhaseId(value);
        }

        public static PlanPhaseId Empty => new(0);

        public override string ToString() => Value.ToString();
    }
}
