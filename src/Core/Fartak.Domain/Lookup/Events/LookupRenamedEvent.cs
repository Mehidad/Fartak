using Fartak.Domain.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fartak.Domain.Lookup.Events
{
    // تغییر عنوان نمایشی یک مقدار فهرست
    public sealed record LookupRenamedEvent(
        long lookupId,
        string domain,
        string code,
        string oldTitle,
        string newTitle) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
