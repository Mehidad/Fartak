using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.PlanVersion.Events
{
    // Events/PlanVersionRejectedEvent.cs — رد برنامه توسط هر یک از دو نقش؛ نسخه پایانی است
    public sealed record PlanVersionRejectedEvent(
        long planVersionId, long caseId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
