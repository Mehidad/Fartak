using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.CostEntry.Events
{
    // تغییر شرح هزینه
    public sealed record CostEntryDescriptionChangedEvent(
        long costEntryId,
        long caseId,
        string? oldDescription,
        string? newDescription) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
