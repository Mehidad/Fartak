using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.CostEntry.Events
{
    // اصلاح تاریخ هزینه
    public sealed record CostEntryOccurredAtChangedEvent(
        long costEntryId,
        long caseId,
        DateTime oldOccurredAt,
        DateTime newOccurredAt) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
