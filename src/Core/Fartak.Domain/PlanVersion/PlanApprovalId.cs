using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.PlanVersion
{
    // شناسه یک ردیف از جدول plan_approvals (تأیید دوگانه کارشناس/کشاورز) — فرزند Aggregate «PlanVersion»
    public sealed record PlanApprovalId
    {
        public long Value { get; }

        private PlanApprovalId(long value) => Value = value;

        public static PlanApprovalId From(long value)
        {
            if (value <= 0)
                throw new DomainValidationException(ErrorCodes.InvalidId, "PlanApprovalId must be positive.");
            return new PlanApprovalId(value);
        }

        public static PlanApprovalId Empty => new(0);

        public override string ToString() => Value.ToString();
    }
}
