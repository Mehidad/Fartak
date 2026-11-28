using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.ProcurementBatch.Events
{
    // انتخاب/تعویض تأمین‌کنندهٔ هدف سفارش تجمیعی
    public sealed record ProcurementBatchSupplierAssignedEvent(
        long batchId,
        long? oldSupplierId,
        long? newSupplierId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
