using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Case;
using Fartak.Domain.Common;
using Fartak.Domain.CostCategory;
using Fartak.Domain.CostEntry.Events;
using Fartak.Domain.Framework;
using Fartak.Domain.PlanVersion;
using Fartak.Domain.User;

namespace Fartak.Domain.CostEntry
{
    // ردیف هزینهٔ واقعی (جدول cost_entries) — هزینه‌های هر پرونده؛ مقایسه با هزینهٔ برآوردی
    // خط برنامهٔ مرتبط، «انحراف بودجه» را می‌سازد (منظرهٔ گزارش‌گیری v_case_cost_summary).
    //
    // پیوند اختیاری به خط برنامه (plan_line_id): هزینهٔ مرتبط با اجرای یک اقدام مشخص؛
    // هزینهٔ عمومی پرونده بدون پیوند ثبت می‌شود (مثل حمل یا نیروی کار عمومی).
    //
    // طبق قاعده timestamp/is_active: اسکیما فقط created_at دارد و updated_at ندارد
    // → فقط CreatedAt نگه می‌شود؛ بدون UpdatedAt؛ بدون Activate/Deactivate.
    // مبلغ به ریال و عدد صحیح (bigint) ذخیره می‌شود؛ CHECK amount_rial >= 0 → صفر مجاز است.
    public sealed class CostEntry : AggregateRoot<CostEntryId>
    {
        public const int MaxDescriptionLength = 2000;   // description از نوع text

        public CaseId CaseId { get; private set; }                     // پروندهٔ صاحب هزینه
        public PlanLineId? PlanLineId { get; private set; }            // خط برنامهٔ مرتبط (اختیاری)
        public CostCategoryId CategoryId { get; private set; }         // سرفصل هزینه (درخت دولایه)
        public long AmountRial { get; private set; }                   // مبلغ هزینه (ریال) — >= 0
        public DateTime OccurredAt { get; private set; }               // تاریخ هزینه (date)
        public string? Description { get; private set; }
        public LookupId? SourceId { get; private set; }                // lookup: cost_source (اختیاری)
        public UserId? CreatedBy { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private CostEntry() { } // برای EF Core

        private CostEntry(
            CostEntryId id,
            CaseId caseId,
            PlanLineId? planLineId,
            CostCategoryId categoryId,
            long amountRial,
            DateTime occurredAt,
            string? description,
            LookupId? sourceId,
            UserId? createdBy)
            : base(id)
        {
            CaseId = caseId;
            PlanLineId = planLineId;
            CategoryId = categoryId;
            AmountRial = amountRial;
            OccurredAt = occurredAt;
            Description = description;
            SourceId = sourceId;
            CreatedBy = createdBy;
            CreatedAt = DateTime.UtcNow;
        }

        public static Result<CostEntry> Create(
            CostEntryId id,
            CaseId caseId,
            CostCategoryId categoryId,
            long amountRial,
            DateTime occurredAtUtc,
            PlanLineId? planLineId = null,
            LookupId? sourceId = null,
            string? description = null,
            UserId? createdBy = null)
        {
            if (id is null)
                return Result.Failure<CostEntry>(CostEntryErrors.NullReference);

            if (caseId == CaseId.Empty)
                return Result.Failure<CostEntry>(CostEntryErrors.CaseNotSpecified);

            if (categoryId == CostCategoryId.Empty)
                return Result.Failure<CostEntry>(CostEntryErrors.CategoryNotSpecified);

            if (amountRial < 0)
                return Result.Failure<CostEntry>(CostEntryErrors.InvalidAmount);

            if (occurredAtUtc.Date > DateTime.UtcNow.Date)
                return Result.Failure<CostEntry>(CostEntryErrors.OccurredAtInFuture);

            var trimmedDescription = description?.Trim();
            if (trimmedDescription is not null && trimmedDescription.Length > MaxDescriptionLength)
                return Result.Failure<CostEntry>(CostEntryErrors.DescriptionTooLong());

            var costEntry = new CostEntry(
                id, caseId, planLineId, categoryId, amountRial, occurredAtUtc,
                trimmedDescription, sourceId, createdBy);

            costEntry.AddDomainEvent(new CostEntryCreatedEvent(
                id.Value,
                caseId.Value,
                planLineId?.Value,
                categoryId.Value,
                amountRial,
                occurredAtUtc,
                trimmedDescription,
                sourceId?.Value,
                createdBy?.Value));

            return Result.Success(costEntry);
        }

        // اصلاح مبلغ هزینه — مقایسه با برآورد خط برنامه در لایه Application بازمحاسبه می‌شود
        public Result UpdateAmount(long newAmountRial)
        {
            if (newAmountRial < 0)
                return Result.Failure(CostEntryErrors.InvalidAmount);

            if (newAmountRial == AmountRial)
                return Result.Success();

            var oldAmountRial = AmountRial;
            AmountRial = newAmountRial;

            AddDomainEvent(new CostEntryAmountUpdatedEvent(
                Id.Value,
                CaseId.Value,
                PlanLineId?.Value,
                oldAmountRial,
                newAmountRial));

            return Result.Success();
        }

        // تغییر سرفصل هزینه (درخت دولایهٔ cost_categories)
        public Result ChangeCategory(CostCategoryId newCategoryId)
        {
            if (newCategoryId == CostCategoryId.Empty)
                return Result.Failure(CostEntryErrors.CategoryNotSpecified);

            if (newCategoryId == CategoryId)
                return Result.Success();

            var oldCategoryId = CategoryId;
            CategoryId = newCategoryId;

            AddDomainEvent(new CostEntryCategoryChangedEvent(
                Id.Value,
                CaseId.Value,
                oldCategoryId.Value,
                newCategoryId.Value));

            return Result.Success();
        }

        // پیوند هزینه به خط برنامه — برای محاسبهٔ انحراف بودجهٔ همان اقدام
        public Result LinkToPlanLine(PlanLineId planLineId)
        {
            if (planLineId is null)
                return Result.Failure(CostEntryErrors.NullReference);

            if (PlanLineId is not null)
                return Result.Failure(CostEntryErrors.AlreadyLinkedToPlanLine);

            PlanLineId = planLineId;

            AddDomainEvent(new CostEntryPlanLineLinkedEvent(
                Id.Value,
                CaseId.Value,
                planLineId.Value));

            return Result.Success();
        }

        // برداشتن پیوند خط برنامه — هزینهٔ عمومی پرونده می‌شود
        public Result UnlinkFromPlanLine()
        {
            if (PlanLineId is null)
                return Result.Failure(CostEntryErrors.PlanLineNotLinked);

            var oldPlanLineId = PlanLineId;
            PlanLineId = null;

            AddDomainEvent(new CostEntryPlanLineUnlinkedEvent(
                Id.Value,
                CaseId.Value,
                oldPlanLineId.Value));

            return Result.Success();
        }

        // تغییر منبع هزینه (lookup: cost_source) — با null قابل پاک‌سازی است
        public Result SetSource(LookupId? sourceId)
        {
            if (sourceId == SourceId)
                return Result.Failure(CostEntryErrors.SourceUnchanged);

            var oldSourceId = SourceId;
            SourceId = sourceId;

            AddDomainEvent(new CostEntrySourceChangedEvent(
                Id.Value,
                CaseId.Value,
                oldSourceId?.Value,
                sourceId?.Value));

            return Result.Success();
        }

        // اصلاح تاریخ هزینه — نباید در آینده باشد
        public Result SetOccurredAt(DateTime newOccurredAtUtc)
        {
            if (newOccurredAtUtc.Date > DateTime.UtcNow.Date)
                return Result.Failure(CostEntryErrors.OccurredAtInFuture);

            if (newOccurredAtUtc.Date == OccurredAt.Date)
                return Result.Success();

            var oldOccurredAt = OccurredAt;
            OccurredAt = newOccurredAtUtc;

            AddDomainEvent(new CostEntryOccurredAtChangedEvent(
                Id.Value,
                CaseId.Value,
                oldOccurredAt,
                newOccurredAtUtc));

            return Result.Success();
        }

        public Result SetDescription(string? description)
        {
            var trimmedDescription = description?.Trim();
            if (trimmedDescription is not null && trimmedDescription.Length > MaxDescriptionLength)
                return Result.Failure(CostEntryErrors.DescriptionTooLong());

            if (trimmedDescription == Description)
                return Result.Failure(CostEntryErrors.DescriptionUnchanged);

            var oldDescription = Description;
            Description = trimmedDescription;

            AddDomainEvent(new CostEntryDescriptionChangedEvent(
                Id.Value,
                CaseId.Value,
                oldDescription,
                trimmedDescription));

            return Result.Success();
        }
    }
}
