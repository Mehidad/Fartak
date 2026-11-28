using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.SupplierInvoice.Events
{
    // تغییر Meta فاکتور تأمین‌کننده (jsonb آزاد)
    public sealed record SupplierInvoiceMetaChangedEvent(
        long supplierInvoiceId,
        long batchId,
        string? oldMeta,
        string? newMeta) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
