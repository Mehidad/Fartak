using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.PlanVersion.Events
{
    // Events/PlanPhaseRenamedEvent.cs — تغییر عنوان فاز
    public sealed record PlanPhaseRenamedEvent(
        long planPhaseId, long planVersionId, string oldTitle, string newTitle) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
