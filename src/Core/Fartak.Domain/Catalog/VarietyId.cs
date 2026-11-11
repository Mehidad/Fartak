using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Catalog
{
    // شناسه یک ردیف از جدول varieties (رقم‌های هر محصول)
    // قرارداد پیش از ساخت Aggregate «Variety» در گام‌های بعد
    public sealed record VarietyId
    {
        public long Value { get; }

        private VarietyId(long value) => Value = value;

        public static VarietyId From(long value)
        {
            if (value <= 0)
                throw new DomainValidationException(ErrorCodes.InvalidId, "VarietyId must be positive.");
            return new VarietyId(value);
        }

        public static VarietyId Empty => new(0);

        public override string ToString() => Value.ToString();
    }
}
