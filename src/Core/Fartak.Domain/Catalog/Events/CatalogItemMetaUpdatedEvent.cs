using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Catalog.Events
{
    // به‌روزرسانی Meta (jsonb آزاد) یک قلم کاتالوگ
    public sealed record CatalogItemMetaUpdatedEvent(
        long catalogItemId,
        string code) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
