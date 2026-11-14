using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Catalog.Events
{
    // غیرفعال‌سازی قلم کاتالوگ — ارجاع‌های موجود معتبر می‌مانند
    public sealed record CatalogItemDeactivatedEvent(
        long catalogItemId,
        string code) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
