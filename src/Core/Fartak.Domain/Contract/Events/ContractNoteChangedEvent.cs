using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Contract.Events
{
    // تغییر یادداشت قرارداد
    public sealed record ContractNoteChangedEvent(
        long contractId,
        long caseId,
        string? oldNote,
        string? newNote) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
