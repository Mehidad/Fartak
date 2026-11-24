using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Payment
{
    // شناسه یک ردیف از جدول payments (پرداخت‌های مرحله‌ای/علی‌الحساب/تسویه در چارچوب پرونده)
    public sealed record PaymentId
    {
        public long Value { get; }

        private PaymentId(long value) => Value = value;

        public static PaymentId From(long value)
        {
            if (value <= 0)
                throw new DomainValidationException(ErrorCodes.InvalidId, "PaymentId must be positive.");
            return new PaymentId(value);
        }

        public static PaymentId Empty => new(0);

        public override string ToString() => Value.ToString();
    }
}
