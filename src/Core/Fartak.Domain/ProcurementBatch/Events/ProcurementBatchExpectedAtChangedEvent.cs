using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.ProcurementBatch.Events
{
    // به‌روزرسانی زمان تحویل موردانتظار سفارش
    public sealed record ProcurementBatchExpectedAtChangedEvent(
        long batchId,
        DateTime? oldExpectedAt,
        DateTime? newExpectedAt) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
