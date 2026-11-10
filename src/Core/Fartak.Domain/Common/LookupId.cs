using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Common
{
   
        // شناسه یک ردیف از جدول lookups (فهرست‌های پایه سیستم: منبع آب، نوع آبیاری، نوع بازدید، ...)
        public sealed record LookupId
        {
            public long Value { get; }

            private LookupId(long value) => Value = value;

            public static LookupId From(long value)
            {
                if (value <= 0)
                    throw new DomainValidationException(ErrorCodes.InvalidId, "LookupId must be positive.");
                return new LookupId(value);
            }

            public static LookupId Empty => new(0);

            public override string ToString() => Value.ToString();
        }
    
}
