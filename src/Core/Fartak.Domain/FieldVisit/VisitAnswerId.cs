using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.FieldVisit
{
    // شناسه یک پاسخ ثبت‌شده در بازدید میدانی (جدول visit_answers)
    public sealed record VisitAnswerId
    {
        public long Value { get; }

        private VisitAnswerId(long value) => Value = value;

        public static VisitAnswerId From(long value)
        {
            if (value <= 0)
                throw new DomainValidationException(ErrorCodes.InvalidId, "VisitAnswerId must be positive.");
            return new VisitAnswerId(value);
        }

        public static VisitAnswerId Empty => new(0);

        public override string ToString() => Value.ToString();
    }
}
