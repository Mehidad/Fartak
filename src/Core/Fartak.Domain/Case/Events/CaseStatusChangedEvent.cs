using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Case.Events
{
    // تغییر وضعیت پرونده — همراه با رکورد ثبت‌شده در تاریخچه (case_status_logs)
    public sealed record CaseStatusChangedEvent(
        long caseId,
        long? fromStatusId,
        long toStatusId,
        long? changedBy,
        bool isAutomatic) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
