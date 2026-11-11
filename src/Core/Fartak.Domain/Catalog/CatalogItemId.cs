using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Catalog
{
    // شناسه یک ردیف از جدول catalog_items (کاتالوگ اقلام و خدمات)
    // قرارداد پیش از ساخت Aggregate «CatalogItem» در گام‌های بعد
    public sealed record CatalogItemId
    {
        public long Value { get; }

        private CatalogItemId(long value) => Value = value;

        public static CatalogItemId From(long value)
        {
            if (value <= 0)
                throw new DomainValidationException(ErrorCodes.InvalidId, "CatalogItemId must be positive.");
            return new CatalogItemId(value);
        }

        public static CatalogItemId Empty => new(0);

        public override string ToString() => Value.ToString();
    }
}
