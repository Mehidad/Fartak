using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Delivery
{
    // شناسه یک ردیف از جدول delivery_items (ریز اقلام هر تحویل)
    public sealed record DeliveryItemId
    {
        public long Value { get; }

        private DeliveryItemId(long value) => Value = value;

        public static DeliveryItemId From(long value)
        {
            if (value <= 0)
                throw new DomainValidationException(ErrorCodes.InvalidId, "DeliveryItemId must be positive.");
            return new DeliveryItemId(value);
        }

        public static DeliveryItemId Empty => new(0);

        public override string ToString() => Value.ToString();
    }
}
