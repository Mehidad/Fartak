using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Catalog.Events
{
    // ایجاد قلم جدید در کاتالوگ اقلام و خدمات
    public sealed record CatalogItemCreatedEvent(
        long catalogItemId,
        string code,
        string title,
        long itemTypeId,
        long? defaultUnitId,
        long? refPriceRial,
        long? priceSourceId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
