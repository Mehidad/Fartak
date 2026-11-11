using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Case
{
    // شناسه یک ردیف از جدول case_status_logs (تاریخچه تغییر وضعیت)
    public sealed record CaseStatusLogId
    {
        public long Value { get; }

        private CaseStatusLogId(long value) => Value = value;

        public static CaseStatusLogId From(long value)
        {
            if (value <= 0)
                throw new DomainValidationException(ErrorCodes.InvalidId, "CaseStatusLogId must be positive.");
            return new CaseStatusLogId(value);
        }

        public static CaseStatusLogId Empty => new(0);

        public override string ToString() => Value.ToString();
    }
}
