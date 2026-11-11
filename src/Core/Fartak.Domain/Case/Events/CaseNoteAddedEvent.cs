using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Case.Events
{
    // افزودن یادداشت آزاد به پرونده
    public sealed record CaseNoteAddedEvent(
        long caseId,
        long noteId,
        long authorId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
