using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.CostEntry.Events
{
    // ثبت هزینهٔ واقعی جدید در پرونده
    public sealed record CostEntryCreatedEvent(
        long costEntryId,
        long caseId,
        long? planLineId,
        long categoryId,
        long amountRial,
        DateTime occurredAt,
        string? description,
        long? sourceId,
        long? createdBy) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
