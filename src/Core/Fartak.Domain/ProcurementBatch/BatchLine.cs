using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;
using Fartak.Domain.PlanVersion;
using Fartak.Domain.Unit;

namespace Fartak.Domain.ProcurementBatch
{
    // ردیف سفارش تجمیعی (جدول batch_lines) — موجودیت فرزندِ ProcurementBatch؛
    // تخصیص هر خط برنامهٔ چند پرونده به سفارش + پاسخ قیمت واحد تأمین‌کننده.
    // فقط از طریق خود Aggregate ساخته/ویرایش می‌شود؛ batch_id ستون صریح ندارد
    // (مطابق الگوی CaseNote) و با navigation در EF نگاشت می‌شود.
    // یکتایی (batch_id, plan_line_id) درون تجمع کنترل می‌شود.
    public sealed class BatchLine : Entity<BatchLineId>
    {
        public PlanLineId PlanLineId { get; private set; }        // خط برنامهٔ مبدأ تخصیص
        public decimal Quantity { get; private set; }             // numeric(12,3) > 0
        public UnitId? UnitId { get; private set; }               // واحد مقدار (اختیاری)
        public long? UnitPriceRial { get; private set; }          // قیمت واحد توافقی (ریال) — پاسخ تأمین‌کننده
        public long? LineTotalRial { get; private set; }          // جمع ردیف = quantity × unit_price (بازمحاسبهٔ دامنه)
        public string? Note { get; private set; }

        private BatchLine() { } // برای EF Core

        private BatchLine(
            BatchLineId id,
            PlanLineId planLineId,
            decimal quantity,
            UnitId? unitId,
            string? note)
        {
            Id = id;
            PlanLineId = planLineId;
            Quantity = quantity;
            UnitId = unitId;
            Note = note;
        }

        // فقط ریشه تجمع صدا می‌زند؛ اعتبارسنجی مقدار/یکتایی پیش‌تر در ریشه انجام شده است
        internal static BatchLine Create(
            BatchLineId id,
            PlanLineId planLineId,
            decimal quantity,
            UnitId? unitId,
            string? note)
        {
            if (id is null || planLineId is null || quantity <= 0)
                throw new DomainValidationException(
                    "ProcurementBatch.LineInvalid",
                    "شناسه ردیف، خط برنامه یا مقدار ردیف سفارش ارسال نشده/نامعتبر است.");

            return new BatchLine(id, planLineId, quantity, unitId, note);
        }

        internal void SetUnit(UnitId? unitId)
            => UnitId = unitId;

        // ثبت پاسخ قیمت واحد تأمین‌کننده؛ جمع ردیف بلافاصله بازمحاسبه می‌شود
        internal void SetPricing(long? unitPriceRial)
        {
            UnitPriceRial = unitPriceRial;
            LineTotalRial = unitPriceRial is null
                ? null
                : (long)Math.Round(Quantity * unitPriceRial.Value, MidpointRounding.AwayFromZero);
        }

        internal void SetNote(string? note)
            => Note = note;
    }
}
