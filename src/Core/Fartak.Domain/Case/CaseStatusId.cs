using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Case
{
    // شناسه یک ردیف از جدول case_statuses (وضعیت‌های چرخه پرونده)
    // وضعیت‌ها داده مرجعِ قابل تنظیم هستند (نه enum سخت)؛ قواعد گذار در جدول case_status_transitions
    // نگه داشته می‌شود و در لایه Application بررسی می‌گردد.
    public sealed record CaseStatusId
    {
        public long Value { get; }

        private CaseStatusId(long value) => Value = value;

        public static CaseStatusId From(long value)
        {
            if (value <= 0)
                throw new DomainValidationException(ErrorCodes.InvalidId, "CaseStatusId must be positive.");
            return new CaseStatusId(value);
        }

        public static CaseStatusId Empty => new(0);

        public override string ToString() => Value.ToString();
    }
}
