using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.PlanVersion.Events
{
    // Events/PlanLineUpdatedEvent.cs — ویرایش خط؛ هزینهٔ جدید برای بازمحاسبه بودجه
    public sealed record PlanLineUpdatedEvent(
        long planLineId, long planVersionId, long estimatedCostRial) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
