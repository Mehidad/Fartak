using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.SupplierInvoice
{
    // شناسه یک ردیف از جدول supplier_invoices (فاکتور تجمیعی تأمین‌کننده برای هر سفارش)
    public sealed record SupplierInvoiceId
    {
        public long Value { get; }

        private SupplierInvoiceId(long value) => Value = value;

        public static SupplierInvoiceId From(long value)
        {
            if (value <= 0)
                throw new DomainValidationException(ErrorCodes.InvalidId, "SupplierInvoiceId must be positive.");
            return new SupplierInvoiceId(value);
        }

        public static SupplierInvoiceId Empty => new(0);

        public override string ToString() => Value.ToString();
    }
}
