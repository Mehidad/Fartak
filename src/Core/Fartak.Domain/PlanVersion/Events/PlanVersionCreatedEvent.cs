using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.PlanVersion.Events
{
    // Events/PlanVersionCreatedEvent.cs — ساخت نسخهٔ جدید برنامه (همیشه پیش‌نویس)
    public sealed record PlanVersionCreatedEvent(
        long planVersionId, long caseId, int versionNo) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
