using Fartak.Domain.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fartak.Domain.Lookup.Events
{
    // غیرفعال‌سازی یک مقدار فهرست — ارجاع‌های موجود (LookupId در رکوردهای دیگر) معتبر می‌مانند
    public sealed record LookupDeactivatedEvent(
        long lookupId,
        string domain,
        string code) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
