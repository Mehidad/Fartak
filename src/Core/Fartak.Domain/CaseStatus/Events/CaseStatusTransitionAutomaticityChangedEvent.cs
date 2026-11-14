using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.CaseStatus.Events
{
    // تغییر خودکار/دستی‌بودن اجرای گذار
    public sealed record CaseStatusTransitionAutomaticityChangedEvent(
        long caseStatusId,
        string code,
        long transitionId,
        long toStatusId,
        bool isAutomatic) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
