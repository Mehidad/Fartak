using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.PlanVersion.Events
{
    // Events/PlanVersionSubmittedForApprovalEvent.cs — ارسال برای تأیید دوگانه (منجمدشدن نسخه)؛
    // مبنای اعلان به کشاورز برای بررسی برنامه
    public sealed record PlanVersionSubmittedForApprovalEvent(
        long planVersionId, long caseId, long expertApprovalId, long farmerApprovalId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
