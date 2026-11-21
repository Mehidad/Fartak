using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.PlanVersion.Events
{
    // Events/PlanLineExecutionRecordedEvent.cs — ثبت اجرای واقعی خط؛ مبنای پیشرفت (v_case_progress)
    // و انحراف بودجه (actual در برابر estimated)
    public sealed record PlanLineExecutionRecordedEvent(
        long planLineId, long planVersionId, DateTime executedAt) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
