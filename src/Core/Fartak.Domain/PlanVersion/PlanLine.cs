using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Catalog;
using Fartak.Domain.Common;
using Fartak.Domain.Framework;
using Fartak.Domain.Unit;
using Fartak.Domain.User.Fartak.Domain.User;

namespace Fartak.Domain.PlanVersion
{
    // خط برنامه (جدول plan_lines) — قلب اجرای برنامه: قلمِ تأمینی یا اقدامِ اجرایی.
    // دو دسته میدان با قاعده انجماد متفاوت:
    //   ۱) میدان‌های برنامه‌ریزی (مقدار، واحد، مبنای مقدار، روش تأمین، مسئول، تاریخ‌ها،
    //      هزینه برآوردی، یادداشت، ترتیب) — فقط در نسخهٔ پیش‌نویس؛ والد اجازه می‌دهد.
    //   ۲) میدان‌های اجرا (actual_qty / actual_cost_rial / executed_at / executed_by /
    //      وضعیت خط) — فقط روی نسخهٔ تأییدشده، از طریق والد.
    // line_type شناسه معنایی است (CHECK اسکیما: item/action) و پس از ایجاد تغییر نمی‌کند —
    // تغییر نوع یعنی حذف خط و ساخت خط جدید.
    public class PlanLine : Entity<PlanLineId>
    {
        public PlanPhaseId? PhaseId { get; private set; }      // فاز صاحب خط (اختیاری)
        public string LineType { get; private set; }           // item | action (تغییرناپذیر)
        public CatalogItemId? CatalogItemId { get; private set; } // فقط برای item
        public string Title { get; private set; }              // snapshot عنوان کاتالوگ یا متن آزاد
        public decimal Quantity { get; private set; }          // numeric(12,3) > 0
        public LookupId QtyBasisId { get; private set; }       // lookup: qty_basis (per_hectare/total)
        public UnitId UnitId { get; private set; }
        public LookupId? SupplyMethodId { get; private set; }  // lookup: supply_method (فقط item)
        public LookupId ResponsibleId { get; private set; }    // lookup: responsible_type
        public DateOnly? PlannedDate { get; private set; }
        public DateOnly? PlannedEndDate { get; private set; }
        public long EstimatedCostRial { get; private set; }    // ریال — >= 0
        public string? ExpertNote { get; private set; }
        public LookupId StatusId { get; private set; }         // lookup: line_status
         public decimal? ActualQty { get; private set; }        // numeric(12,3) — میدان اجرا
        public long? ActualCostRial { get; private set; }      // میدان اجرا
        public DateTime? ExecutedAt { get; private set; }      // میدان اجرا
        public UserId? ExecutedBy { get; private set; }        // میدان اجرا
        public int SortOrder { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        private PlanLine() { } // برای EF Core

        internal PlanLine(
            PlanLineId id,
            PlanPhaseId? phaseId,
            string lineType,
            CatalogItemId? catalogItemId,
            string title,
            decimal quantity,
            LookupId qtyBasisId,
            UnitId unitId,
            LookupId? supplyMethodId,
            LookupId responsibleId,
            DateOnly? plannedDate,
            DateOnly? plannedEndDate,
            long estimatedCostRial,
            string? expertNote,
            int sortOrder,
            LookupId statusId
            )
        {
            Id = id;
            PhaseId = phaseId;
            LineType = lineType;
            CatalogItemId = catalogItemId;
            Title = title;
            Quantity = quantity;
            QtyBasisId = qtyBasisId;
            UnitId = unitId;
            SupplyMethodId = supplyMethodId;
            ResponsibleId = responsibleId;
            PlannedDate = plannedDate;
            PlannedEndDate = plannedEndDate;
            EstimatedCostRial = estimatedCostRial;
            ExpertNote = expertNote;
            SortOrder = sortOrder;
            StatusId = statusId;
            
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = CreatedAt;
        }

        // ساخت خط جدید — فقط از طریق PlanVersion.AddLine (پیش‌اعتبارسنجی در والد)
        internal static PlanLine Create(
            PlanLineId id,
            PlanPhaseId? phaseId,
            string lineType,
            CatalogItemId? catalogItemId,
            string title,
            decimal quantity,
            LookupId qtyBasisId,
            UnitId unitId,
            LookupId? supplyMethodId,
            LookupId responsibleId,
            DateOnly? plannedDate,
            DateOnly? plannedEndDate,
            long estimatedCostRial,
            string? expertNote,
            int sortOrder,
            LookupId statusId)
            => new PlanLine(id, phaseId, lineType, catalogItemId, title, quantity,
                qtyBasisId, unitId, supplyMethodId, responsibleId,
                plannedDate, plannedEndDate, estimatedCostRial, expertNote, sortOrder, statusId);

        // ویرایش میدان‌های برنامه‌ریزی — فقط والد (در وضعیت draft) صدا می‌زند؛
        // line_type و میدان‌های اجرا دست‌نخورده می‌مانند
        internal void UpdatePlanning(
            PlanPhaseId? phaseId,
            CatalogItemId? catalogItemId,
            string title,
            decimal quantity,
            LookupId qtyBasisId,
            UnitId unitId,
            LookupId? supplyMethodId,
            LookupId responsibleId,
            DateOnly? plannedDate,
            DateOnly? plannedEndDate,
            long estimatedCostRial,
            string? expertNote,
            int sortOrder)
        {
            PhaseId = phaseId;
            CatalogItemId = catalogItemId;
            Title = title;
            Quantity = quantity;
            QtyBasisId = qtyBasisId;
            UnitId = unitId;
            SupplyMethodId = supplyMethodId;
            ResponsibleId = responsibleId;
            PlannedDate = plannedDate;
            PlannedEndDate = plannedEndDate;
            EstimatedCostRial = estimatedCostRial;
            ExpertNote = expertNote;
            SortOrder = sortOrder;
            UpdatedAt = DateTime.UtcNow;
        }

        // گذار وضعیت خط — فقط والد (روی نسخهٔ تأییدشده) صدا می‌زند
        internal void SetStatus(LookupId newStatusId)
        {
            StatusId = newStatusId;
            UpdatedAt = DateTime.UtcNow;
        }

        // ثبت اجرای واقعی + نهایی‌کردن خط با وضعیت «انجام‌شده» — فقط والد
        internal void RecordExecution(
            LookupId doneStatusId,
            decimal? actualQty,
            long? actualCostRial,
            DateTime executedAt,
            UserId? executedBy)
        {
            ActualQty = actualQty;
            ActualCostRial = actualCostRial;
            ExecutedAt = executedAt;
            ExecutedBy = executedBy;
            StatusId = doneStatusId;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
