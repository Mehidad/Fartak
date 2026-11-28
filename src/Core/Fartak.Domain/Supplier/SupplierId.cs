using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Supplier
{
    // شناسه یک ردیف از جدول suppliers (تأمین‌کنندگانِ سفارش تجمیعی نهاده‌ها)
    public sealed record SupplierId
    {
        public long Value { get; }

        private SupplierId(long value) => Value = value;

        public static SupplierId From(long value)
        {
            if (value <= 0)
                throw new DomainValidationException(ErrorCodes.InvalidId, "SupplierId must be positive.");
            return new SupplierId(value);
        }

        public static SupplierId Empty => new(0);

        public override string ToString() => Value.ToString();
    }
}
