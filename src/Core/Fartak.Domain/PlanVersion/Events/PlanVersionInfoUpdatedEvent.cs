using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.PlanVersion.Events
{
    // Events/PlanVersionInfoUpdatedEvent.cs — ویرایش اطلاعات سراسری نسخه در پیش‌نویس
    public sealed record PlanVersionInfoUpdatedEvent(long planVersionId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
