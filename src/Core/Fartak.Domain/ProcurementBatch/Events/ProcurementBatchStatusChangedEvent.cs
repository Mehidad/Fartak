using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.ProcurementBatch.Events
{
    // تغییر وضعیت سفارش تجمیعی (lookup: batch_status)
    public sealed record ProcurementBatchStatusChangedEvent(
        long batchId,
        long oldStatusId,
        long newStatusId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
