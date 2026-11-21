using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.PlanVersion.Events
{
    // Events/PlanLineAddedEvent.cs — افزودن خط؛ هزینه برآوردی برای به‌روزرسانی بودجه در read model
    public sealed record PlanLineAddedEvent(
        long planLineId, long planVersionId, string lineType, long estimatedCostRial) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
