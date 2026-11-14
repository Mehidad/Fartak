using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.CaseStatus.Events
{
    // اعلام/لغو وضعیت پایانی — فقط برای وضعیت بدون گذار خروجی ممکن است
    public sealed record CaseStatusTerminalityChangedEvent(
        long caseStatusId,
        string code,
        bool isTerminal) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
