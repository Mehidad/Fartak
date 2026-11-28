using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.SupplierInvoice
{
    // خطاهای تجمع فاکتور تأمین‌کننده
    public static class SupplierInvoiceErrors
    {
        public static readonly Error NullReference = Error.Create(
            "SupplierInvoice.NullReference",
            "مقادیر اجباری فاکتور تأمین‌کننده ارسال نشده است.");

        public static readonly Error BatchNotSpecified = Error.Create(
            "SupplierInvoice.BatchNotSpecified",
            "برای ثبت فاکتور باید سفارش تجمیعی مشخص باشد.");

        public static readonly Error InvoiceNoNotSpecified = Error.Create(
            "SupplierInvoice.InvoiceNoNotSpecified",
            "شماره فاکتور تأمین‌کننده باید مشخص باشد.");

        public static Error InvoiceNoTooLong(int length)
            => Error.Create(
                code: "SupplierInvoice.InvoiceNoTooLong",
                message: "شماره فاکتور تأمین‌کننده نمی‌تواند بیش از ۶۰ نویسه باشد.",
                details: new Dictionary<string, object>
                {
                    ["Length"] = length,
                    ["MaxLength"] = SupplierInvoice.MaxInvoiceNoLength
                });

        public static readonly Error IssuedAtInFuture = Error.Create(
            "SupplierInvoice.IssuedAtInFuture",
            "تاریخ صدور فاکتور نمی‌تواند در آینده باشد.");

        public static readonly Error InvalidTotal = Error.Create(
            "SupplierInvoice.InvalidTotal",
            "جمع فاکتور باید بزرگ‌تر از صفر باشد.");

        public static readonly Error StatusNotSpecified = Error.Create(
            "SupplierInvoice.StatusNotSpecified",
            "وضعیت فاکتور باید مشخص باشد.");

        public static readonly Error SameStatus = Error.Create(
            "SupplierInvoice.SameStatus",
            "وضعیت جدید فاکتور با وضعیت فعلی یکسان است.");

        public static readonly Error MetaUnchanged = Error.Create(
            "SupplierInvoice.MetaUnchanged",
            "مقدار جدید Meta با مقدار فعلی یکسان است.");
    }
}
