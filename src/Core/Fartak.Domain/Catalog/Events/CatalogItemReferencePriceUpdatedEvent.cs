using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Catalog.Events
{
    // ثبت/تغییر/پاک‌کردن قیمت مرجع قلم — با پاک‌کردن قیمت، priceUpdatedAt تهی است
    public sealed record CatalogItemReferencePriceUpdatedEvent(
        long catalogItemId,
        string code,
        long? refPriceRial,
        long? priceSourceId,
        DateTime? priceUpdatedAt) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
