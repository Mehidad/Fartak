using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.ProcurementBatch.Events
{
    // تغییر یادداشت سفارش تجمیعی
    public sealed record ProcurementBatchNoteChangedEvent(
        long batchId,
        string? oldNote,
        string? newNote) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
