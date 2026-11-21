using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.PlanVersion.Events
{
    // Events/PlanVersionApprovedEvent.cs — گذار خودکار به تأییدشده بعد از دو تأیید؛
    // Application با این رویداد نسخه‌های قبلی را MarkSuperseded می‌کند و اجرا باز می‌شود
    public sealed record PlanVersionApprovedEvent(
        long planVersionId, long caseId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
