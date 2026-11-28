using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.CostEntry.Events
{
    // تغییر منبع هزینه (lookup: cost_source)
    public sealed record CostEntrySourceChangedEvent(
        long costEntryId,
        long caseId,
        long? oldSourceId,
        long? newSourceId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
