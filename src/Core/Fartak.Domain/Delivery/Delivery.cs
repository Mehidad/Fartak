using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Case;
using Fartak.Domain.Common;
using Fartak.Domain.Delivery.Events;
using Fartak.Domain.Framework;
using Fartak.Domain.PlanVersion;
using Fartak.Domain.ProcurementBatch;
using Fartak.Domain.User;

namespace Fartak.Domain.Delivery
{
    // تحویل (جدول deliveries) + فرزندش ریز اقلام تحویل (delivery_items)
    // — تجمع «تحویل اقلام به مزرعه»: حمل، زمان تحویل و تأیید دوگانهٔ تحویل‌گیرنده.
    //
    // چرخهٔ حیات با lookup مدیریت می‌شود (domain=delivery_status) — الگوی VisitSchedule/Payment؛
    // اعتبارسنجی گذارها با کد وضعیت در لایه Application انجام می‌شود و تجمع فقط LookupId نگه می‌دارد.
    // گاردهای دامنه‌ای وابسته به کد lookup اینجا نداریم؛ به‌جای آن «زمان‌ها» نقش نگهبان را بازی می‌کنند:
    // تأیید کشاورز (confirmed_at) تنها پس از ثبت زمان تحویل (delivered_at) ممکن است
    // و هر دو یک‌بار برای همیشه ثبت می‌شوند (NotDeliveredYet / AlreadyConfirmed).
    //
    // طبق قاعده timestamp/is_active: UpdatedAt نگه می‌شود (created_at دارد)؛
    // is_active نداریم → Activate/Deactivate ندارد؛ چرخهٔ حیات با status_id است.
    public sealed class Delivery : AggregateRoot<DeliveryId>
    {
        public const int MaxCarrierLength = 120;        // varchar(120)
        public const int MaxVehiclePlateLength = 20;    // varchar(20)
        public const int MaxNoteLength = 2000;          // note از نوع text
        public const int QuantityScale = 3;             // numeric(12,3) در delivery_items

        private readonly List<DeliveryItem> _items = new();

        public CaseId CaseId { get; private set; }                     // پروندهٔ مقصد تحویل
        public ProcurementBatchId? BatchId { get; private set; }       // سفارش تجمیعی مبدأ (اختیاری)
        public LookupId StatusId { get; private set; }                 // lookup: delivery_status
        public DateTime? DeliveredAt { get; private set; }             // زمان تحویل
        public UserId? ReceivedBy { get; private set; }                // تحویل‌گیرنده
        public DateTime? ConfirmedAt { get; private set; }             // تأیید تحویل توسط کشاورز
        public string? Carrier { get; private set; }                   // شرکت حمل
        public string? VehiclePlate { get; private set; }              // پلاک خودرو
        public long TransportCostRial { get; private set; }            // هزینهٔ حمل (ریال) — >= 0
        public string? Note { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public IReadOnlyCollection<DeliveryItem> Items => _items.AsReadOnly();

        private Delivery() { } // برای EF Core

        private Delivery(
            DeliveryId id,
            CaseId caseId,
            ProcurementBatchId? batchId,
            LookupId statusId,
            string? note)
            : base(id)
        {
            CaseId = caseId;
            BatchId = batchId;
            StatusId = statusId;
            TransportCostRial = 0;                                     // DEFAULT 0
            Note = note;
            CreatedAt = DateTime.UtcNow;
        }

        public static Result<Delivery> Create(
            DeliveryId id,
            CaseId caseId,
            LookupId statusId,
            ProcurementBatchId? batchId = null,
            string? note = null)
        {
            if (id is null)
                return Result.Failure<Delivery>(DeliveryErrors.NullReference);

            if (caseId == CaseId.Empty)
                return Result.Failure<Delivery>(DeliveryErrors.CaseNotSpecified);

            if (statusId == LookupId.Empty)
                return Result.Failure<Delivery>(DeliveryErrors.StatusNotSpecified);

            var trimmedNote = note?.Trim();
            if (trimmedNote is not null && trimmedNote.Length > MaxNoteLength)
                return Result.Failure<Delivery>(DeliveryErrors.NoteTooLong);

            var delivery = new Delivery(id, caseId, batchId, statusId, trimmedNote);

            delivery.AddDomainEvent(new DeliveryCreatedEvent(
                id.Value,
                caseId.Value,
                batchId?.Value,
                statusId.Value,
                trimmedNote));

            return Result.Success(delivery);
        }

        // افزودن ردیف تحویل — هر ردیف باید به خط برنامه یا ردیف سفارش تجمیعی ارجاع داشته باشد
        public Result AddItem(
            DeliveryItemId deliveryItemId,
            decimal quantity,
            PlanLineId? planLineId = null,
            BatchLineId? batchLineId = null,
            string? note = null)
        {
            if (deliveryItemId is null)
                return Result.Failure(DeliveryErrors.ItemNullReference);

            if (planLineId is null && batchLineId is null)
                return Result.Failure(DeliveryErrors.ItemNoReference);

            if (quantity <= 0)
                return Result.Failure(DeliveryErrors.InvalidItemQuantity);

            var trimmedNote = note?.Trim();
            if (trimmedNote is not null && trimmedNote.Length > MaxNoteLength)
                return Result.Failure(DeliveryErrors.ItemNoteTooLong);

            var item = DeliveryItem.Create(deliveryItemId, planLineId, batchLineId, quantity, trimmedNote);
            _items.Add(item);

            AddDomainEvent(new DeliveryItemAddedEvent(
                Id.Value,
                item.Id.Value,
                item.PlanLineId?.Value,
                item.BatchLineId?.Value,
                item.Quantity,
                item.Note));

            return Result.Success();
        }

        // حذف ردیف تحویل — پیش از تأیید نهایی تحویل (کنترل وضعیت در Application)
        public Result RemoveItem(DeliveryItemId deliveryItemId)
        {
            var item = FindItem(deliveryItemId);
            if (item is null)
                return Result.Failure(DeliveryErrors.ItemNotFound(deliveryItemId?.Value ?? 0));

            _items.Remove(item);

            AddDomainEvent(new DeliveryItemRemovedEvent(
                Id.Value,
                item.Id.Value,
                item.PlanLineId?.Value,
                item.BatchLineId?.Value));

            return Result.Success();
        }

        public Result UpdateItemNote(DeliveryItemId deliveryItemId, string? note)
        {
            var item = FindItem(deliveryItemId);
            if (item is null)
                return Result.Failure(DeliveryErrors.ItemNotFound(deliveryItemId?.Value ?? 0));

            var trimmedNote = note?.Trim();
            if (trimmedNote is not null && trimmedNote.Length > MaxNoteLength)
                return Result.Failure(DeliveryErrors.ItemNoteTooLong);

            var oldNote = item.Note;
            item.SetNote(trimmedNote);

            AddDomainEvent(new DeliveryItemNoteChangedEvent(
                Id.Value,
                item.Id.Value,
                oldNote,
                trimmedNote));

            return Result.Success();
        }

        // ثبت زمان تحویل + تحویل‌گیرنده — یک‌بار برای همیشه؛ دوباره‌کاری ممکن نیست
        public Result MarkDelivered(DateTime deliveredAtUtc, UserId? receivedBy = null)
        {
            if (DeliveredAt is not null)
                return Result.Failure(DeliveryErrors.AlreadyDelivered);

            if (deliveredAtUtc > DateTime.UtcNow)
                return Result.Failure(DeliveryErrors.DeliveredAtInFuture);

            DeliveredAt = deliveredAtUtc;
            ReceivedBy = receivedBy;

            AddDomainEvent(new DeliveryMarkedDeliveredEvent(
                Id.Value,
                CaseId.Value,
                deliveredAtUtc,
                receivedBy?.Value));

            return Result.Success();
        }

        // تأیید تحویل توسط کشاورز — تنها پس از ثبت زمان تحویل و فقط یک‌بار
        public Result Confirm(DateTime confirmedAtUtc)
        {
            if (DeliveredAt is null)
                return Result.Failure(DeliveryErrors.NotDeliveredYet);

            if (ConfirmedAt is not null)
                return Result.Failure(DeliveryErrors.AlreadyConfirmed);

            if (confirmedAtUtc > DateTime.UtcNow)
                return Result.Failure(DeliveryErrors.ConfirmedAtInFuture);

            ConfirmedAt = confirmedAtUtc;

            AddDomainEvent(new DeliveryConfirmedEvent(
                Id.Value,
                CaseId.Value,
                DeliveredAt.Value,
                confirmedAtUtc));

            return Result.Success();
        }

        // گذار وضعیت تحویل — اعتبارسنجی ماشین وضعیت با کد lookup در لایه Application
        public Result ChangeStatus(LookupId newStatusId)
        {
            if (newStatusId == LookupId.Empty)
                return Result.Failure(DeliveryErrors.StatusNotSpecified);

            if (newStatusId == StatusId)
                return Result.Failure(DeliveryErrors.SameStatus);

            var oldStatusId = StatusId;
            StatusId = newStatusId;

            AddDomainEvent(new DeliveryStatusChangedEvent(
                Id.Value,
                CaseId.Value,
                oldStatusId.Value,
                newStatusId.Value));

            return Result.Success();
        }

        // هزینهٔ حمل (ریال) — >= 0
        public Result SetTransportCost(long transportCostRial)
        {
            if (transportCostRial < 0)
                return Result.Failure(DeliveryErrors.InvalidTransportCost);

            if (transportCostRial == TransportCostRial)
                return Result.Success();

            var oldTransportCostRial = TransportCostRial;
            TransportCostRial = transportCostRial;

            AddDomainEvent(new DeliveryTransportCostChangedEvent(
                Id.Value,
                CaseId.Value,
                oldTransportCostRial,
                transportCostRial));

            return Result.Success();
        }

        // اطلاعات حمل: شرکت حمل و پلاک خودرو؛ با null قابل پاک‌سازی‌اند
        public Result SetCarrier(string? carrier, string? vehiclePlate = null)
        {
            var normalizedCarrier = carrier?.Trim();
            if (normalizedCarrier is not null && normalizedCarrier.Length > MaxCarrierLength)
                return Result.Failure(DeliveryErrors.CarrierTooLong(normalizedCarrier.Length));

            var normalizedVehiclePlate = vehiclePlate?.Trim();
            if (normalizedVehiclePlate is not null && normalizedVehiclePlate.Length > MaxVehiclePlateLength)
                return Result.Failure(DeliveryErrors.VehiclePlateTooLong(normalizedVehiclePlate.Length));

            if (normalizedCarrier == Carrier && normalizedVehiclePlate == VehiclePlate)
                return Result.Success();

            var oldCarrier = Carrier;
            var oldVehiclePlate = VehiclePlate;
            Carrier = normalizedCarrier;
            VehiclePlate = normalizedVehiclePlate;

            AddDomainEvent(new DeliveryCarrierSetEvent(
                Id.Value,
                CaseId.Value,
                oldCarrier,
                normalizedCarrier,
                oldVehiclePlate,
                normalizedVehiclePlate));

            return Result.Success();
        }

        public Result SetNote(string? note)
        {
            var trimmedNote = note?.Trim();
            if (trimmedNote is not null && trimmedNote.Length > MaxNoteLength)
                return Result.Failure(DeliveryErrors.NoteTooLong);

            var oldNote = Note;
            Note = trimmedNote;

            AddDomainEvent(new DeliveryNoteChangedEvent(
                Id.Value,
                CaseId.Value,
                oldNote,
                trimmedNote));

            return Result.Success();
        }

        private DeliveryItem? FindItem(DeliveryItemId? deliveryItemId)
            => deliveryItemId is null
                ? null
                : _items.FirstOrDefault(i => i.Id.Value == deliveryItemId.Value);
    }
}
