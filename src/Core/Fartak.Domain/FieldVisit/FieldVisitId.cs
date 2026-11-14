using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.FieldVisit
{
    // شناسه یک بازدید میدانی انجام‌شده (جدول field_visits)
    public sealed record FieldVisitId
    {
        public long Value { get; }

        private FieldVisitId(long value) => Value = value;

        public static FieldVisitId From(long value)
        {
            if (value <= 0)
                throw new DomainValidationException(ErrorCodes.InvalidId, "FieldVisitId must be positive.");
            return new FieldVisitId(value);
        }

        public static FieldVisitId Empty => new(0);

        public override string ToString() => Value.ToString();
    }
}
