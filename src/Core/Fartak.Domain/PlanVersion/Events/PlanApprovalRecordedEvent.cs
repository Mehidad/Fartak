using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.PlanVersion.Events
{
    // Events/PlanApprovalRecordedEvent.cs — ثبت تصمیم یک نقش (ممکن است نسخه هنوز تمام نشده باشد)
    public sealed record PlanApprovalRecordedEvent(
        long planApprovalId, long planVersionId, string approverRole, string decisionCode) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
