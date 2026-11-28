using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.InsurancePolicy
{
    // شناسه یک ردیف از جدول insurance_policies (بیمه‌نامهٔ محصول هر پرونده)
    public sealed record InsurancePolicyId
    {
        public long Value { get; }

        private InsurancePolicyId(long value) => Value = value;

        public static InsurancePolicyId From(long value)
        {
            if (value <= 0)
                throw new DomainValidationException(ErrorCodes.InvalidId, "InsurancePolicyId must be positive.");
            return new InsurancePolicyId(value);
        }

        public static InsurancePolicyId Empty => new(0);

        public override string ToString() => Value.ToString();
    }
}
