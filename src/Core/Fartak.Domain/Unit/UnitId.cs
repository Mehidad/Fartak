using Fartak.Domain.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fartak.Domain.Unit
{
    // شناسه یک ردیف از جدول units (واحدهای اندازه‌گیری: کیلوگرم، لیتر، هکتار، ساعت-ماشین، ...)
    // در Aggregate «Unit» (Unit.cs) استفاده می‌شود؛ تجمع‌های بعدی (آیتم چک‌لیست، خطوط طرح،
    // پاس‌های عددی بازدید، خطوط خرید و ...) با همین شناسه به واحد ارجاع می‌دهند.
    public sealed record UnitId
    {
        public long Value { get; }

        private UnitId(long value) => Value = value;

        public static UnitId From(long value)
        {
            if (value <= 0)
                throw new DomainValidationException(ErrorCodes.InvalidId, "UnitId must be positive.");
            return new UnitId(value);
        }

        public static UnitId Empty => new(0);

        public override string ToString() => Value.ToString();
    }
}
