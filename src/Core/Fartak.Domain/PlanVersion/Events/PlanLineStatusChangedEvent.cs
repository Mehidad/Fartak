using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.PlanVersion.Events
{
    // Events/PlanLineStatusChangedEvent.cs — گذار وضعیت خط بدون ثبت اجرا (شروع کار/تأخیر/حذف/لغو)
    public sealed record PlanLineStatusChangedEvent(
        long planLineId, long planVersionId, string toStatusCode) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
