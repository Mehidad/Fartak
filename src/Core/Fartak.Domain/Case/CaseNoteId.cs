using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Case
{
    // شناسه یک ردیف از جدول case_notes (یادداشت‌های آزاد روی پرونده)
    public sealed record CaseNoteId
    {
        public long Value { get; }

        private CaseNoteId(long value) => Value = value;

        public static CaseNoteId From(long value)
        {
            if (value <= 0)
                throw new DomainValidationException(ErrorCodes.InvalidId, "CaseNoteId must be positive.");
            return new CaseNoteId(value);
        }

        public static CaseNoteId Empty => new(0);

        public override string ToString() => Value.ToString();
    }
}
