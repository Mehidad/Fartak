using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Delivery
{
    // شناسه یک ردیف از جدول deliveries (تحویل به مزرعه/کشاورز + اطلاعات حمل)
    public sealed record DeliveryId
    {
        public long Value { get; }

        private DeliveryId(long value) => Value = value;

        private DeliveryId(long value) => Value = value;

        public static DeliveryId From(long value)
        {
            if (value <= 0)
                throw new DomainValidationException(ErrorCodes.InvalidId, "DeliveryId must be positive.");
            return new DeliveryId(value);
        }

        public static DeliveryId Empty => new(0);

        public override string ToString() => Value.ToString();
    }
}
