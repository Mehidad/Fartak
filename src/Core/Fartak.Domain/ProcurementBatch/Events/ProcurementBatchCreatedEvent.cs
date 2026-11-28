using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.ProcurementBatch.Events
{
    // ثبت سفارش تجمیعی جدید
    public sealed record ProcurementBatchCreatedEvent(
        long batchId,
        string batchCode,
        long? supplierId,
        long statusId,
        DateTime? requestedAt,
        DateTime? expectedAt,
        string? note,
        long? createdBy) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
