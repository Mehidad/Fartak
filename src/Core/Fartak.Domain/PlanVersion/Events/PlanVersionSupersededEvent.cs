using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.PlanVersion.Events
{
    // Events/PlanVersionSupersededEvent.cs — جایگزین‌شدن نسخه توسط نسخهٔ جدیدتر
    public sealed record PlanVersionSupersededEvent(
        long planVersionId, long caseId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
