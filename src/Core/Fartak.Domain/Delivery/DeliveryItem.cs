using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;
using Fartak.Domain.PlanVersion;
using Fartak.Domain.ProcurementBatch;

namespace Fartak.Domain.Delivery
{
    // ردیف تحویل (جدول delivery_items) — موجودیت فرزندِ Delivery؛
    // ریز اقلام تحویل‌شده؛ هر ردیف یا به خط برنامه یا به ردیف سفارش تجمیعی ارجاع دارد
    // (معادل CHECK (plan_line_id IS NOT NULL OR batch_line_id IS NOT NULL) اسکیما).
    // فقط از طریق خود Aggregate ساخته می‌شود؛ delivery_id ستون صریح ندارد
    // (مطابق الگوی CaseNote) و با navigation در EF نگاشت می‌شود.
    public sealed class DeliveryItem : Entity<DeliveryItemId>
    {
        public PlanLineId? PlanLineId { get; private set; }           // خط برنامهٔ مبدأ (اختیاری)
        public BatchLineId? BatchLineId { get; private set; }         // ردیف سفارش تجمیعی مبدأ (اختیاری)
        public decimal Quantity { get; private set; }                 // numeric(12,3) > 0
        public string? Note { get; private set; }

        private DeliveryItem() { } // برای EF Core

        private DeliveryItem(
            DeliveryItemId id,
            PlanLineId? planLineId,
            BatchLineId? batchLineId,
            decimal quantity,
            string? note)
        {
            Id = id;
            PlanLineId = planLineId;
            BatchLineId = batchLineId;
            Quantity = quantity;
            Note = note;
        }

        // فقط ریشه تجمع صدا می‌زند؛ اعتبارسنجی ارجاع/مقدار پیش‌تر در ریشه انجام شده است
        internal static DeliveryItem Create(
            DeliveryItemId id,
            PlanLineId? planLineId,
            BatchLineId? batchLineId,
            decimal quantity,
            string? note)
        {
            if (id is null || quantity <= 0 || (planLineId is null && batchLineId is null))
                throw new DomainValidationException(
                    "Delivery.ItemInvalid",
                    "شناسه ردیف، مقدار یا ارجاع مبدأ ردیف تحویل ارسال نشده/نامعتبر است.");

            return new DeliveryItem(id, planLineId, batchLineId, quantity, note);
        }

        internal void SetNote(string? note)
            => Note = note;
    }
}
