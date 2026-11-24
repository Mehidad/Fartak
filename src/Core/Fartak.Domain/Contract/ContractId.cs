using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Contract
{
    // شناسه یک ردیف از جدول contracts (قراردادهای عقدشده در چارچوب پرونده‌های کشاورزی)
    // در Aggregate «Contract» (Contract.cs) استفاده می‌شود؛ پرداخت‌ها و حساب امانی
    // با همین شناسه به قرارداد ارجاع می‌دهند.
    public sealed record ContractId
    {
        public long Value { get; }

        private ContractId(long value) => Value = value;

        public static ContractId From(long value)
        {
            if (value <= 0)
                throw new DomainValidationException(ErrorCodes.InvalidId, "ContractId must be positive.");
            return new ContractId(value);
        }

        public static ContractId Empty => new(0);

        public override string ToString() => Value.ToString();
    }
}
