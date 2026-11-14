using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.CaseStatus.Events
{
    // تغییر فهرست نقش‌های مجاز گذار (فهرست خالی = فقط سیستم)
    public sealed record CaseStatusTransitionRolesChangedEvent(
        long caseStatusId,
        string code,
        long transitionId,
        long toStatusId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
