using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Common;
using Fartak.Domain.Framework;
using Fartak.Domain.ProcurementBatch;
using Fartak.Domain.SupplierInvoice.Events;

namespace Fartak.Domain.SupplierInvoice
{
    // فاکتور تجمیعی تأمین‌کننده (جدول supplier_invoices) — فاکتور رسمی هر سفارش تجمیعی؛
    // مبنای کنترل مالی سفارش در لایه مالی (paymentها جداگانه رهگیری می‌شوند).
    //
    // چرخهٔ حیات با lookup مدیریت می‌شود (domain=invoice_status) — الگوی Payment؛
    // اعتبارسنجی گذارها با کد وضعیت در لایه Application انجام می‌شود.
    //
    // طبق قاعده timestamp/is_active: UpdatedAt نگه می‌شود (created_at دارد)؛
    // is_active نداریم → Activate/Deactivate ندارد.
    // meta (jsonb آزاد) — اعتبارسنجی ساختار در لایه Application انجام می‌شود.
    public sealed class SupplierInvoice : AggregateRoot<SupplierInvoiceId>
    {
        public const int MaxInvoiceNoLength = 60;       // varchar(60) NOT NULL

        public ProcurementBatchId BatchId { get; private set; }   // سفارش تجمیعی مرتبط
        public string InvoiceNo { get; private set; }             // شماره فاکتور تأمین‌کننده
        public DateTime IssuedAt { get; private set; }            // تاریخ صدور فاکتور (date)
        public long TotalRial { get; private set; }               // جمع فاکتور (ریال) — > 0
        public LookupId StatusId { get; private set; }            // lookup: invoice_status
        public string? Meta { get; private set; }                 // jsonb آزاد (اعتبارسنجی ساختار در لایه Application)
        public DateTime CreatedAt { get; private set; }

        private SupplierInvoice() { } // برای EF Core

        private SupplierInvoice(
            SupplierInvoiceId id,
            ProcurementBatchId batchId,
            string invoiceNo,
            DateTime issuedAt,
            long totalRial,
            LookupId statusId,
            string? meta)
            : base(id)
        {
            BatchId = batchId;
            InvoiceNo = invoiceNo;
            IssuedAt = issuedAt;
            TotalRial = totalRial;
            StatusId = statusId;
            Meta = meta;
            CreatedAt = DateTime.UtcNow;
        }

        public static Result<SupplierInvoice> Create(
            SupplierInvoiceId id,
            ProcurementBatchId batchId,
            string invoiceNo,
            DateTime issuedAtUtc,
            long totalRial,
            LookupId statusId,
            string? meta = null)
        {
            if (id is null)
                return Result.Failure<SupplierInvoice>(SupplierInvoiceErrors.NullReference);

            if (batchId == ProcurementBatchId.Empty)
                return Result.Failure<SupplierInvoice>(SupplierInvoiceErrors.BatchNotSpecified);

            if (string.IsNullOrWhiteSpace(invoiceNo))
                return Result.Failure<SupplierInvoice>(SupplierInvoiceErrors.InvoiceNoNotSpecified);

            var trimmedInvoiceNo = invoiceNo.Trim();
            if (trimmedInvoiceNo.Length > MaxInvoiceNoLength)
                return Result.Failure<SupplierInvoice>(SupplierInvoiceErrors.InvoiceNoTooLong(trimmedInvoiceNo.Length));

            if (issuedAtUtc.Date > DateTime.UtcNow.Date)
                return Result.Failure<SupplierInvoice>(SupplierInvoiceErrors.IssuedAtInFuture);

            if (totalRial <= 0)
                return Result.Failure<SupplierInvoice>(SupplierInvoiceErrors.InvalidTotal);

            if (statusId == LookupId.Empty)
                return Result.Failure<SupplierInvoice>(SupplierInvoiceErrors.StatusNotSpecified);

            var invoice = new SupplierInvoice(
                id,
                batchId,
                trimmedInvoiceNo,
                issuedAtUtc,
                totalRial,
                statusId,
                NormalizeMeta(meta));

            invoice.AddDomainEvent(new SupplierInvoiceCreatedEvent(
                id.Value,
                batchId.Value,
                invoice.InvoiceNo,
                invoice.IssuedAt,
                totalRial,
                statusId.Value,
                invoice.Meta));

            return Result.Success(invoice);
        }

        // گذار وضعیت فاکتور — اعتبارسنجی ماشین وضعیت با کد lookup در لایه Application
        public Result ChangeStatus(LookupId newStatusId)
        {
            if (newStatusId == LookupId.Empty)
                return Result.Failure(SupplierInvoiceErrors.StatusNotSpecified);

            if (newStatusId == StatusId)
                return Result.Failure(SupplierInvoiceErrors.SameStatus);

            var oldStatusId = StatusId;
            StatusId = newStatusId;

            AddDomainEvent(new SupplierInvoiceStatusChangedEvent(
                Id.Value,
                BatchId.Value,
                oldStatusId.Value,
                newStatusId.Value));

            return Result.Success();
        }

        // اصلاح جمع فاکتور (مثلاً مغایرت با جمع ردیف‌های سفارش) — مبلغ جدید باید مثبت باشد
        public Result CorrectTotal(long newTotalRial)
        {
            if (newTotalRial <= 0)
                return Result.Failure(SupplierInvoiceErrors.InvalidTotal);

            if (newTotalRial == TotalRial)
                return Result.Success();

            var oldTotalRial = TotalRial;
            TotalRial = newTotalRial;

            AddDomainEvent(new SupplierInvoiceTotalCorrectedEvent(
                Id.Value,
                BatchId.Value,
                oldTotalRial,
                newTotalRial));

            return Result.Success();
        }

        // به‌روزرسانی Meta (jsonb آزاد) — با null قابل پاک‌سازی است
        public Result SetMeta(string? meta)
        {
            var normalized = NormalizeMeta(meta);
            if (normalized == Meta)
                return Result.Failure(SupplierInvoiceErrors.MetaUnchanged);

            var oldMeta = Meta;
            Meta = normalized;

            AddDomainEvent(new SupplierInvoiceMetaChangedEvent(
                Id.Value,
                BatchId.Value,
                oldMeta,
                normalized));

            return Result.Success();
        }

        private static string? NormalizeMeta(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
