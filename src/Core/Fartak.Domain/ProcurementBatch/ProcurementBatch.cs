using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Common;
using Fartak.Domain.Framework;
using Fartak.Domain.PlanVersion;
using Fartak.Domain.ProcurementBatch.Events;
using Fartak.Domain.Supplier;
using Fartak.Domain.Unit;
using Fartak.Domain.User;

namespace Fartak.Domain.ProcurementBatch
{
    // سفارش تجمیعی (جدول procurement_batches) + فرزندش ردیف‌های سفارش (batch_lines)
    // — تجمع «تأمین تجمیعی»: خطوط برنامهٔ چند پرونده به یک سفارش برای تأمین‌کننده تبدیل می‌شود
    // و پاسخ قیمت واحد تأمین‌کننده روی هر ردیف ثبت می‌گردد.
    //
    // چرخهٔ حیات با lookup مدیریت می‌شود (domain=batch_status) — الگوی VisitSchedule/Payment؛
    // اعتبارسنجی گذارها با کد وضعیت در لایه Application انجام می‌شود و تجمع فقط LookupId نگه می‌دارد.
    //
    // طبق قاعده timestamp/is_active: UpdatedAt نگه می‌شود (created_at دارد)؛
    // is_active نداریم → Activate/Deactivate ندارد؛ چرخهٔ حیات با status_id است.
    // یکتایی batch_code بین چند تجمع است → در Application با کنترل تکرار تضمین می‌شود.
    public sealed class ProcurementBatch : AggregateRoot<ProcurementBatchId>
    {
        public const int MaxBatchCodeLength = 30;       // varchar(30) NOT NULL UNIQUE
        public const int MaxNoteLength = 2000;          // note از نوع text
        public const int QuantityScale = 3;             // numeric(12,3)

        private readonly List<BatchLine> _lines = new();

        public string BatchCode { get; private set; }             // کد سفارش تجمیعی (یکتا)
        public SupplierId? SupplierId { get; private set; }       // تأمین‌کنندهٔ هدف (اختیاری تا انتخاب)
        public LookupId StatusId { get; private set; }            // lookup: batch_status
        public DateTime? RequestedAt { get; private set; }        // زمان ارسال سفارش به تأمین‌کننده
        public DateTime? ExpectedAt { get; private set; }         // زمان تحویل موردانتظار
        public string? Note { get; private set; }
        public UserId? CreatedBy { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public IReadOnlyCollection<BatchLine> Lines => _lines.AsReadOnly();

        private ProcurementBatch() { } // برای EF Core

        private ProcurementBatch(
            ProcurementBatchId id,
            string batchCode,
            SupplierId? supplierId,
            LookupId statusId,
            DateTime? requestedAt,
            DateTime? expectedAt,
            string? note,
            UserId? createdBy)
            : base(id)
        {
            BatchCode = batchCode;
            SupplierId = supplierId;
            StatusId = statusId;
            RequestedAt = requestedAt;
            ExpectedAt = expectedAt;
            Note = note;
            CreatedBy = createdBy;
            CreatedAt = DateTime.UtcNow;
        }

        public static Result<ProcurementBatch> Create(
            ProcurementBatchId id,
            string batchCode,
            LookupId statusId,
            SupplierId? supplierId = null,
            DateTime? requestedAtUtc = null,
            DateTime? expectedAtUtc = null,
            string? note = null,
            UserId? createdBy = null)
        {
            if (id is null)
                return Result.Failure<ProcurementBatch>(ProcurementBatchErrors.NullReference);

            if (string.IsNullOrWhiteSpace(batchCode))
                return Result.Failure<ProcurementBatch>(ProcurementBatchErrors.BatchCodeNotSpecified);

            var trimmedBatchCode = batchCode.Trim();
            if (trimmedBatchCode.Length > MaxBatchCodeLength)
                return Result.Failure<ProcurementBatch>(ProcurementBatchErrors.BatchCodeTooLong(trimmedBatchCode.Length));

            if (statusId == LookupId.Empty)
                return Result.Failure<ProcurementBatch>(ProcurementBatchErrors.StatusNotSpecified);

            var trimmedNote = note?.Trim();
            if (trimmedNote is not null && trimmedNote.Length > MaxNoteLength)
                return Result.Failure<ProcurementBatch>(ProcurementBatchErrors.NoteTooLong());

            var batch = new ProcurementBatch(
                id, trimmedBatchCode, supplierId, statusId,
                requestedAtUtc, expectedAtUtc, trimmedNote, createdBy);

            batch.AddDomainEvent(new ProcurementBatchCreatedEvent(
                id.Value,
                batch.BatchCode,
                supplierId?.Value,
                statusId.Value,
                requestedAtUtc,
                expectedAtUtc,
                trimmedNote,
                createdBy?.Value));

            return Result.Success(batch);
        }

        // افزودن خط برنامه به سفارش — هر خط برنامه در یک سفارش فقط یک‌بار تخصیص می‌یابد
        public Result AddLine(
            BatchLineId batchLineId,
            PlanLineId planLineId,
            decimal quantity,
            UnitId? unitId = null,
            long? unitPriceRial = null,
            string? note = null)
        {
            if (batchLineId is null || planLineId is null)
                return Result.Failure(ProcurementBatchErrors.LineNullReference);

            if (quantity <= 0)
                return Result.Failure(ProcurementBatchErrors.InvalidQuantity);

            if (_lines.Any(l => l.PlanLineId == planLineId))
                return Result.Failure(ProcurementBatchErrors.LineDuplicated(planLineId.Value));

            var trimmedNote = note?.Trim();
            if (trimmedNote is not null && trimmedNote.Length > MaxNoteLength)
                return Result.Failure(ProcurementBatchErrors.LineNoteTooLong());

            var line = BatchLine.Create(batchLineId, planLineId, quantity, unitId, trimmedNote);
            line.SetPricing(unitPriceRial);
            _lines.Add(line);

            AddDomainEvent(new BatchLineAddedEvent(
                Id.Value,
                line.Id.Value,
                line.PlanLineId.Value,
                line.Quantity,
                line.UnitId?.Value,
                line.UnitPriceRial,
                line.LineTotalRial,
                line.Note));

            return Result.Success();
        }

        // حذف ردیف سفارش — پیش از نهایی‌شدن سفارش (کنترل وضعیت در Application)
        public Result RemoveLine(BatchLineId batchLineId)
        {
            var line = FindLine(batchLineId);
            if (line is null)
                return Result.Failure(ProcurementBatchErrors.LineNotFound(batchLineId?.Value ?? 0));

            _lines.Remove(line);

            AddDomainEvent(new BatchLineRemovedEvent(
                Id.Value,
                line.Id.Value,
                line.PlanLineId.Value));

            return Result.Success();
        }

        // ثبت پاسخ قیمت واحد تأمین‌کننده روی ردیف؛ جمع ردیف بلافاصله بازمحاسبه می‌شود
        public Result UpdateLinePrice(BatchLineId batchLineId, long? unitPriceRial)
        {
            var line = FindLine(batchLineId);
            if (line is null)
                return Result.Failure(ProcurementBatchErrors.LineNotFound(batchLineId?.Value ?? 0));

            var oldUnitPriceRial = line.UnitPriceRial;
            line.SetPricing(unitPriceRial);

            AddDomainEvent(new BatchLinePriceUpdatedEvent(
                Id.Value,
                line.Id.Value,
                line.PlanLineId.Value,
                oldUnitPriceRial,
                line.UnitPriceRial,
                line.LineTotalRial));

            return Result.Success();
        }

        // تغییر وضعیت سفارش تجمیعی — اعتبارسنجی ماشین وضعیت با کد lookup در لایه Application
        public Result ChangeStatus(LookupId newStatusId)
        {
            if (newStatusId == LookupId.Empty)
                return Result.Failure(ProcurementBatchErrors.StatusNotSpecified);

            if (newStatusId == StatusId)
                return Result.Failure(ProcurementBatchErrors.SameStatus);

            var oldStatusId = StatusId;
            StatusId = newStatusId;

            AddDomainEvent(new ProcurementBatchStatusChangedEvent(
                Id.Value,
                oldStatusId.Value,
                newStatusId.Value));

            return Result.Success();
        }

        // انتخاب/تعویض تأمین‌کنندهٔ هدف سفارش؛ با null قابل خالی‌کردن است
        public Result SetSupplier(SupplierId? supplierId)
        {
            if (supplierId == SupplierId)
                return Result.Success();

            var oldSupplierId = SupplierId;
            SupplierId = supplierId;

            AddDomainEvent(new ProcurementBatchSupplierAssignedEvent(
                Id.Value,
                oldSupplierId?.Value,
                supplierId?.Value));

            return Result.Success();
        }

        // به‌روزرسانی زمان تحویل موردانتظار
        public Result Reschedule(DateTime? expectedAtUtc)
        {
            if (expectedAtUtc == ExpectedAt)
                return Result.Success();

            var oldExpectedAt = ExpectedAt;
            ExpectedAt = expectedAtUtc;

            AddDomainEvent(new ProcurementBatchExpectedAtChangedEvent(
                Id.Value,
                oldExpectedAt,
                expectedAtUtc));

            return Result.Success();
        }

        public Result SetNote(string? note)
        {
            var trimmedNote = note?.Trim();
            if (trimmedNote is not null && trimmedNote.Length > MaxNoteLength)
                return Result.Failure(ProcurementBatchErrors.NoteTooLong());

            var oldNote = Note;
            Note = trimmedNote;

            AddDomainEvent(new ProcurementBatchNoteChangedEvent(
                Id.Value,
                oldNote,
                trimmedNote));

            return Result.Success();
        }

        private BatchLine? FindLine(BatchLineId? batchLineId)
            => batchLineId is null
                ? null
                : _lines.FirstOrDefault(l => l.Id.Value == batchLineId.Value);
    }
}
