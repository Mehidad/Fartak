using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Catalog
{
    // شناسه یک ردیف از جدول crops (کاتالوگ محصولات)
    // قرارداد پیش از ساخت Aggregate «Crop» در گام‌های بعد
    public sealed record CropId
    {
        public long Value { get; }

        private CropId(long value) => Value = value;

        public static CropId From(long value)
        {
            if (value <= 0)
                throw new DomainValidationException(ErrorCodes.InvalidId, "CropId must be positive.");
            return new CropId(value);
        }

        public static CropId Empty => new(0);

        public override string ToString() => Value.ToString();
    }
}
