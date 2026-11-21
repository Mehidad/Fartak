using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.PlanVersion.Events
{
    // Events/PlanPhaseAddedEvent.cs — افزودن فاز زمان‌بندی
    public sealed record PlanPhaseAddedEvent(
        long planPhaseId, long planVersionId, string title, int sortOrder) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
