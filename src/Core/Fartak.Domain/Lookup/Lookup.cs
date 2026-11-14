using Fartak.Domain.Common;
using Fartak.Domain.Framework;
using Fartak.Domain.Lookup.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fartak.Domain.Lookup
{
    // فهرست پایه سیستم (جدول lookups) — جایگزین enumهای پراکنده در کل سیستم؛
    // هر مقدار lookup یک ردیف مستقل با PK است و بقیه تجمع‌ها با LookupId (از Common) به آن ارجاع می‌دهند.
    //
    // تصمیم شناسه: از LookupId مشترک پوشه Common استفاده می‌شود؛ چون این قرارداد از ابتدا
    // برای ارجاع سایر تجمع‌ها در Common تعریف شده بود و شناسه تجمع هم همان است.
    //
    // قاعده یکتایی: جفت (Domain, Code) یکتاست (unique index در ERD) و کلید معناییِ ارجاع است —
    // بنابراین این دو پس از ایجاد تغییر نمی‌کنند (متد تغییر ندارند).
    // قابل ویرایش: Title، SortOrder، Meta و IsActive.
    //
    // غیرفعال‌سازی یعنی «از کامبوهای جدید حذف می‌شود»؛ ارجاع‌های موجود در رکوردهای دیگر
    // معتبر می‌مانند (LookupId ذخیره‌شده سر جایش است).
    public class Lookup : AggregateRoot<LookupId>
    {
        public const int MaxDomainLength = 50;
        public const int MaxCodeLength = 50;
        public const int MaxTitleLength = 120;
        public const int MaxSortOrder = 32767; // سقف smallint مطابق ERD

        public string Domain { get; private set; }      // دامنه فهرست مثل visit_type / water_source / payment_method
        public string Code { get; private set; }        // کد مقدار مثل expert_visit / well / cash
        public string Title { get; private set; }       // عنوان نمایشی فارسی
        public int SortOrder { get; private set; }      // ترتیب نمایش در کامبوها
        public bool IsActive { get; private set; }
        public string? Meta { get; private set; }       // jsonb آزاد (اعتبارسنجی ساختار در لایه Application)

        private Lookup() { } // برای EF Core

        private Lookup(LookupId id, string domain, string code, string title, int sortOrder)
        {
            Id = id;
            Domain = domain;
            Code = code;
            Title = title;
            SortOrder = sortOrder;
            IsActive = true;
        }

        public static Result<Lookup> Create(
            LookupId id,
            string domain,
            string code,
            string title,
            int sortOrder = 0,
            string? meta = null)
        {
            if (id is null)
                return Result.Failure<Lookup>(LookupErrors.NullReferenceError());

            var domainValidation = ValidateDomain(domain);
            if (domainValidation.IsFailure)
                return Result.Failure<Lookup>(domainValidation.Error);

            var codeValidation = ValidateCode(code);
            if (codeValidation.IsFailure)
                return Result.Failure<Lookup>(codeValidation.Error);

            var titleValidation = ValidateTitle(title);
            if (titleValidation.IsFailure)
                return Result.Failure<Lookup>(titleValidation.Error);

            var orderValidation = ValidateSortOrder(sortOrder);
            if (orderValidation.IsFailure)
                return Result.Failure<Lookup>(orderValidation.Error);

            var lookup = new Lookup(id, domain.Trim(), code.Trim(), title.Trim(), sortOrder);
            lookup.Meta = string.IsNullOrWhiteSpace(meta) ? null : meta.Trim();

            lookup.AddDomainEvent(new LookupCreatedEvent(
                id.Value, lookup.Domain, lookup.Code, lookup.Title, lookup.SortOrder));
            return Result<Lookup>.Success(lookup);
        }

        // تغییر عنوان نمایشی مقدار فهرست (فرم ویرایش) — دامنه و کد تغییر نمی‌کنند
        public Result Rename(string newTitle)
        {
            var validation = ValidateTitle(newTitle);
            if (validation.IsFailure)
                return validation;

            var trimmed = newTitle.Trim();
            if (trimmed == Title)
                return Result.Failure(LookupErrors.TitleUnchanged());

            var oldTitle = Title;
            Title = trimmed;
            AddDomainEvent(new LookupRenamedEvent(Id.Value, Domain, Code, oldTitle, Title));
            return Result.Success();
        }

        // به‌روزرسانی Meta (jsonb آزاد) — با null قابل پاک‌سازی است
        public Result SetMeta(string? meta)
        {
            var normalized = string.IsNullOrWhiteSpace(meta) ? null : meta.Trim();
            if (normalized == Meta)
                return Result.Failure(LookupErrors.MetaUnchanged());

            Meta = normalized;
            AddDomainEvent(new LookupMetaUpdatedEvent(Id.Value, Domain, Code));
            return Result.Success();
        }

        // تغییر ترتیب نمایش مقدار فهرست (جابه‌جایی در کامبوها)
        public Result SetSortOrder(int newSortOrder)
        {
            var validation = ValidateSortOrder(newSortOrder);
            if (validation.IsFailure)
                return validation;

            if (newSortOrder == SortOrder)
                return Result.Failure(LookupErrors.SortOrderUnchanged());

            var oldOrder = SortOrder;
            SortOrder = newSortOrder;
            AddDomainEvent(new LookupSortOrderChangedEvent(Id.Value, Domain, Code, oldOrder, newSortOrder));
            return Result.Success();
        }

        public Result Activate()
        {
            if (IsActive)
                return Result.Failure(LookupErrors.AlreadyActive(Id.Value));

            IsActive = true;
            AddDomainEvent(new LookupActivatedEvent(Id.Value, Domain, Code));
            return Result.Success();
        }

        public Result Deactivate()
        {
            if (!IsActive)
                return Result.Failure(LookupErrors.AlreadyDeactivated(Id.Value));

            IsActive = false;
            AddDomainEvent(new LookupDeactivatedEvent(Id.Value, Domain, Code));
            return Result.Success();
        }

        // دامنه‌های شناخته‌شده سیستم مطابق یادداشت ERD — برای جلوگیری از رشته‌های پراکنده در کل کد
        public static class WellKnownDomains
        {
            public const string SupplyMethod = "supply_method";
            public const string ResponsibleType = "responsible_type";
            public const string VisitType = "visit_type";
            public const string VisitStatus = "visit_status";
            public const string AnswerType = "answer_type";
            public const string Rating = "rating";
            public const string QtyBasis = "qty_basis";
            public const string LineStatus = "line_status";
            public const string PlanStatus = "plan_status";
            public const string AssignmentSource = "assignment_source";
            public const string AssignmentOutcome = "assignment_outcome";
            public const string BatchStatus = "batch_status";
            public const string DeliveryStatus = "delivery_status";
            public const string InvoiceStatus = "invoice_status";
            public const string PaymentMethod = "payment_method";
            public const string PayeeType = "payee_type";
            public const string CostSource = "cost_source";
            public const string ItemType = "item_type";
            public const string IrrigationType = "irrigation_type";
            public const string WaterSource = "water_source";
            public const string InsuranceStatus = "insurance_status";
            public const string NotificationChannel = "notification_channel";
            public const string NotificationStatus = "notification_status";
            public const string SettlementType = "settlement_type";
            public const string SettlementStatus = "settlement_status";
            public const string ReminderType = "reminder_type";
            public const string ReminderStatus = "reminder_status";
            public const string Decision = "decision";
            public const string GrowthStage = "growth_stage";
        }

        private static Result ValidateDomain(string? domain)
        {
            if (string.IsNullOrWhiteSpace(domain))
                return Result.Failure(LookupErrors.InvalidDomain(domain));

            if (domain.Trim().Length > MaxDomainLength)
                return Result.Failure(LookupErrors.DomainTooLong(domain.Trim()));

            return Result.Success();
        }

        private static Result ValidateCode(string? code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return Result.Failure(LookupErrors.InvalidCode(code));

            if (code.Trim().Length > MaxCodeLength)
                return Result.Failure(LookupErrors.CodeTooLong(code.Trim()));

            return Result.Success();
        }

        private static Result ValidateTitle(string? title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return Result.Failure(LookupErrors.InvalidTitle(title));

            if (title.Trim().Length > MaxTitleLength)
                return Result.Failure(LookupErrors.TitleTooLong(title.Trim()));

            return Result.Success();
        }

        private static Result ValidateSortOrder(int sortOrder)
        {
            if (sortOrder < 0 || sortOrder > MaxSortOrder)
                return Result.Failure(LookupErrors.SortOrderOutOfRange(sortOrder));

            return Result.Success();
        }
    }
}
