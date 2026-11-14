using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Catalog.Events
{
    // تغییر یا پاک‌شدن واحد پیش‌فرض قلم (oldDefaultUnitId پس از پاک‌سازی null است)
    public sealed record CatalogItemDefaultUnitChangedEvent(
        long catalogItemId,
        string code,
        long? oldDefaultUnitId,
        long? newDefaultUnitId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
