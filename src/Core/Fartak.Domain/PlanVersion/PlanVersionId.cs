using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.PlanVersion
{
    // شناسه یک ردیف از جدول plan_versions (نسخه برنامه کشت)
    // در Aggregate «PlanVersion» (PlanVersion.cs) استفاده می‌شود
    public sealed record PlanVersionId
    {
        public long Value { get; }

        private PlanVersionId(long value) => Value = value;

        public static PlanVersionId From(long value)
        {
            if (value <= 0)
                throw new DomainValidationException(ErrorCodes.InvalidId, "PlanVersionId must be positive.");
            return new PlanVersionId(value);
        }

        public static PlanVersionId Empty => new(0);

        public override string ToString() => Value.ToString();
    }
}
