using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Case
{
    // شناسه یک ردیف از جدول case_assignments (تاریخچه تخصیص پرونده به کارشناس)
    public sealed record CaseAssignmentId
    {
        public long Value { get; }

        private CaseAssignmentId(long value) => Value = value;

        public static CaseAssignmentId From(long value)
        {
            if (value <= 0)
                throw new DomainValidationException(ErrorCodes.InvalidId, "CaseAssignmentId must be positive.");
            return new CaseAssignmentId(value);
        }

        public static CaseAssignmentId Empty => new(0);

        public override string ToString() => Value.ToString();
    }
}
