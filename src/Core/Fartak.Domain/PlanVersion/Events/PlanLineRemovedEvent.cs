using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.PlanVersion.Events
{
    // Events/PlanLineRemovedEvent.cs — حذف خط در پیش‌نویس
    public sealed record PlanLineRemovedEvent(
        long planLineId, long planVersionId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
