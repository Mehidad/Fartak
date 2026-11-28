using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.SupplierInvoice.Events
{
    // ثبت فاکتور تأمین‌کننده برای سفارش تجمیعی
    public sealed record SupplierInvoiceCreatedEvent(
        long supplierInvoiceId,
        long batchId,
        string invoiceNo,
        DateTime issuedAt,
        long totalRial,
        long statusId,
        string? meta) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
