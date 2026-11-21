using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.PlanVersion.Events
{
    // Events/PlanVersionWithdrawnEvent.cs — بازگشت به پیش‌نویس (قبل از هر تصمیم)
    public sealed record PlanVersionWithdrawnEvent(long planVersionId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
