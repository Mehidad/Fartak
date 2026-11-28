using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.SupplierInvoice.Events
{
    // تغییر وضعیت فاکتور تأمین‌کننده (lookup: invoice_status)
    public sealed record SupplierInvoiceStatusChangedEvent(
        long supplierInvoiceId,
        long batchId,
        long oldStatusId,
        long newStatusId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
