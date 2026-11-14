using Fartak.Domain.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fartak.Domain.Lookup.Events
{
    public sealed record LookupSortOrderChangedEvent(
        long lookupId,
        string domain,
        string code,
        int oldSortOrder,
        int newSortOrder) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
