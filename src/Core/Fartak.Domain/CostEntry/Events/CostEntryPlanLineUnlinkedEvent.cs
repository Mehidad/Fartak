using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.CostEntry.Events
{
    // برداشتن پیوند خط برنامه — هزینهٔ عمومی پرونده می‌شود
    public sealed record CostEntryPlanLineUnlinkedEvent(
        long costEntryId,
        long caseId,
        long oldPlanLineId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
