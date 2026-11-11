using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Case
{
    // شناسه یک ردیف از جدول cases (پرونده: یک قطعه × یک محصول × یک فصل)
    public sealed record CaseId
    {
        public long Value { get; }

        private CaseId(long value) => Value = value;

        public static CaseId From(long value)
        {
            if (value <= 0)
                throw new DomainValidationException(ErrorCodes.InvalidId, "CaseId must be positive.");
            return new CaseId(value);
        }

        public static CaseId Empty => new(0);

        public override string ToString() => Value.ToString();
    }
}
