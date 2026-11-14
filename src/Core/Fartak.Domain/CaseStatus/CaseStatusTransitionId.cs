using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.CaseStatus
{
    // شناسه یک ردیف از جدول case_status_transitions (قواعد گذار وضعیت)
    public sealed record CaseStatusTransitionId
    {
        public long Value { get; }

        private CaseStatusTransitionId(long value) => Value = value;

        public static CaseStatusTransitionId From(long value)
        {
            if (value <= 0)
                throw new DomainValidationException(ErrorCodes.InvalidId, "CaseStatusTransitionId must be positive.");
            return new CaseStatusTransitionId(value);
        }

        public static CaseStatusTransitionId Empty => new(0);

        public override string ToString() => Value.ToString();
    }
}
