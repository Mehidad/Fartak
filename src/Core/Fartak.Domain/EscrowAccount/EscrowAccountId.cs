using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.EscrowAccount
{
    // شناسه یک ردیف از جدول escrow_accounts (حساب امانی/پانزده‌درصدی قراردادها)
    public sealed record EscrowAccountId
    {
        public long Value { get; }

        private EscrowAccountId(long value) => Value = value;

        public static EscrowAccountId From(long value)
        {
            if (value <= 0)
                throw new DomainValidationException(ErrorCodes.InvalidId, "EscrowAccountId must be positive.");
            return new EscrowAccountId(value);
        }

        public static EscrowAccountId Empty => new(0);

        public override string ToString() => Value.ToString();
    }
}
