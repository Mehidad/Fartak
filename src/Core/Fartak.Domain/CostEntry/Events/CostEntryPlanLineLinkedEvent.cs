using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.CostEntry.Events
{
    // پیوند هزینه به خط برنامه (برای انحراف بودجهٔ همان اقدام)
    public sealed record CostEntryPlanLineLinkedEvent(
        long costEntryId,
        long caseId,
        long planLineId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
