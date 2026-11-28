using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.SupplierInvoice.Events
{
    // اصلاح جمع فاکتور تأمین‌کننده
    public sealed record SupplierInvoiceTotalCorrectedEvent(
        long supplierInvoiceId,
        long batchId,
        long oldTotalRial,
        long newTotalRial) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
