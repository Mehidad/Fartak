using Fartak.Domain.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fartak.Domain.Lookup.Events
{
    // فعال‌سازی مجدد یک مقدار فهرست
    public sealed record LookupActivatedEvent(
        long lookupId,
        string domain,
        string code) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
