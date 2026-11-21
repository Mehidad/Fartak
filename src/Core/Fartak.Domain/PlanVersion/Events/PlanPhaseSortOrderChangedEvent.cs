using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.PlanVersion.Events
{
    // Events/PlanPhaseSortOrderChangedEvent.cs — جابه‌جایی ترتیب فاز
    public sealed record PlanPhaseSortOrderChangedEvent(
        long planPhaseId, long planVersionId, int oldSortOrder, int newSortOrder) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
