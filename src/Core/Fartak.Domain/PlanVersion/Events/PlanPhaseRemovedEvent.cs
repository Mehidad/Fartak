using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.PlanVersion.Events
{
    // Events/PlanPhaseRemovedEvent.cs — حذف فاز (فقط فازِ بدون خط)
    public sealed record PlanPhaseRemovedEvent(
        long planPhaseId, long planVersionId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
