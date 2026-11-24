using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Payment.Events
{
    // تغییر یادداشت پرداخت
    public sealed record PaymentNoteChangedEvent(
        long paymentId,
        long caseId,
        string? oldNote,
        string? newNote) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
