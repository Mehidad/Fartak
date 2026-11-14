using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Common;
using Fartak.Domain.Framework;
using Fartak.Domain.Unit;

namespace Fartak.Domain.Catalog
{
    // قلم کاتالوگ اقلام و خدمات (جدول catalog_items) — بذر/کود/سم/خدمت/ماشین‌آلات؛
    // خطوط طرح (plan_lines.catalog_item_id) و خریدهای آینده با CatalogItemId به آن ارجاع می‌دهند.
    //
    // قواعد تغییرناپذیری: کد قلم کلید معنایی ارجاع است (unique در ERD) و پس از ایجاد تغییر نمی‌کند؛
    // نوع قلم (item_type — مرجع lookup با دامنه item_type) هم ماهیت قلم را تعیین می‌کند و
    // تغییر آن معنای ارجاع‌های تاریخی را می‌شکند — بنابراین متد تغییر نوع وجود ندارد.
    // (اگر نوع قلم اشتباه ثبت شده باشد، مسیر درست: غیرفعال‌سازی قلم و ثبت قلم جدید)
    //
    // قاعده قیمت: قیمت مرجع (bigint ریال) اختیاری است اما همیشه جفتِ منبع قیمت است
    // (lookup با دامنه price_source)؛ قیمت بدون منبع و منبع بدون قیمت هر دو رد می‌شوند.
    // استعلام دوره‌ای از سرویس بیرونی هم از همین متد SetReferencePrice (با منبع api) عبور می‌کند.
    //
    // مطابق ERD جدول ستون created_at ندارد؛ UpdatedAt هنگام ایجاد مقداردهی و با هر تغییر به‌روز می‌شود.
    // غیرفعال‌سازی یعنی «از کامبوهای انتخاب قلم جدید حذف می‌شود»؛ ارجاع‌های موجود معتبر می‌مانند.
    public class CatalogItem : AggregateRoot<CatalogItemId>
    {
        public const int MaxCodeLength = 40;    // varchar(40) در ERD
        public const int MaxTitleLength = 150;  // varchar(150) در ERD

        public string Code { get; private set; }               // یکتا — مثل WHT-SEED-SIRV / FRT-UREA
        public string Title { get; private set; }              // عنوان نمایشی فارسی
        public LookupId ItemTypeId { get; private set; }       // lookup: item_type — immutable
        public UnitId? DefaultUnitId { get; private set; }     // واحد پیش‌فرض پیشنهادی خطوط طرح/خرید
        public long? ReferencePriceRial { get; private set; }  // قیمت مرجع به ریال
        public LookupId? PriceSourceId { get; private set; }   // lookup: price_source (manual/api)
        public DateTime? PriceUpdatedAt { get; private set; }  // زمان آخرین ثبت/تغییر قیمت
        public bool IsActive { get; private set; }
        public string? Meta { get; private set; }              // jsonb آزاد (اعتبارسنجی ساختار در لایه Application)
        public DateTime UpdatedAt { get; private set; }

        private CatalogItem() { } // برای EF Core

        private CatalogItem(CatalogItemId id, string code, string title, LookupId itemTypeId,
            UnitId? defaultUnitId, long? refPriceRial, LookupId? priceSourceId)
        {
            Id = id;
            Code = code;
            Title = title;
            ItemTypeId = itemTypeId;
            DefaultUnitId = defaultUnitId;
            ReferencePriceRial = refPriceRial;
            PriceSourceId = priceSourceId;
            PriceUpdatedAt = refPriceRial.HasValue ? DateTime.UtcNow : null;
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public static Result<CatalogItem> Create(
            CatalogItemId id,
            string code,
            string title,
            LookupId itemTypeId,
            UnitId? defaultUnitId = null,
            long? refPriceRial = null,
            LookupId? priceSourceId = null,
            string? meta = null)
        {
            if (id is null || itemTypeId is null)
                return Result.Failure<CatalogItem>(CatalogItemErrors.NullReferenceError());

            var codeValidation = ValidateCode(code);
            if (codeValidation.IsFailure)
                return Result.Failure<CatalogItem>(codeValidation.Error);

            var titleValidation = ValidateTitle(title);
            if (titleValidation.IsFailure)
                return Result.Failure<CatalogItem>(titleValidation.Error);

            var priceValidation = ValidatePricePair(refPriceRial, priceSourceId);
            if (priceValidation.IsFailure)
                return Result.Failure<CatalogItem>(priceValidation.Error);

            var item = new CatalogItem(id, code.Trim(), title.Trim(), itemTypeId,
                defaultUnitId, refPriceRial, priceSourceId);
            item.Meta = NormalizeMeta(meta);

            item.AddDomainEvent(new CatalogItemCreatedEvent(
                id.Value, item.Code, item.Title, item.ItemTypeId.Value,
                item.DefaultUnitId?.Value, item.ReferencePriceRial, item.PriceSourceId?.Value));
            return Result<CatalogItem>.Success(item);
        }

        // تغییر عنوان نمایشی قلم (فرم مدیریت کاتالوگ) — کد و نوع قلم تغییر نمی‌کنند
        public Result Rename(string newTitle)
        {
            var validation = ValidateTitle(newTitle);
            if (validation.IsFailure)
                return validation;

            var trimmed = newTitle.Trim();
            if (trimmed == Title)
                return Result.Failure(CatalogItemErrors.TitleUnchanged());

            var oldTitle = Title;
            Title = trimmed;
            UpdatedAt = DateTime.UtcNow;
            AddDomainEvent(new CatalogItemRenamedEvent(Id.Value, Code, oldTitle, Title));
            return Result.Success();
        }

        // تعیین/تغییر/پاک‌کردن واحد پیش‌فرض قلم — مقدار تهی یعنی بدون واحد پیش‌فرض
        public Result SetDefaultUnit(UnitId? newUnitId)
        {
            if (newUnitId?.Value == DefaultUnitId?.Value)
                return Result.Failure(CatalogItemErrors.UnitUnchanged());

            var oldUnitId = DefaultUnitId;
            DefaultUnitId = newUnitId;
            UpdatedAt = DateTime.UtcNow;
            AddDomainEvent(new CatalogItemDefaultUnitChangedEvent(
                Id.Value, Code, oldUnitId?.Value, newUnitId?.Value));
            return Result.Success();
        }

        // ثبت/تغییر/پاک‌کردن قیمت مرجع به‌همراه منبع آن — این دو همیشه جفت‌اند؛
        // با ثبت قیمت PriceUpdatedAt به زمان جاری تنظیم و با پاک‌کردن قیمت تهی می‌شود
        public Result SetReferencePrice(long? refPriceRial, LookupId? priceSourceId)
        {
            var validation = ValidatePricePair(refPriceRial, priceSourceId);
            if (validation.IsFailure)
                return validation;

            if (refPriceRial == ReferencePriceRial && priceSourceId?.Value == PriceSourceId?.Value)
                return Result.Failure(CatalogItemErrors.ReferenceUnchanged());

            ReferencePriceRial = refPriceRial;
            PriceSourceId = priceSourceId;
            PriceUpdatedAt = refPriceRial.HasValue ? DateTime.UtcNow : null;
            UpdatedAt = DateTime.UtcNow;
            AddDomainEvent(new CatalogItemReferencePriceUpdatedEvent(
                Id.Value, Code, ReferencePriceRial, PriceSourceId?.Value, PriceUpdatedAt));
            return Result.Success();
        }

        // به‌روزرسانی Meta (jsonb آزاد) — با null قابل پاک‌سازی است
        public Result SetMeta(string? meta)
        {
            var normalized = NormalizeMeta(meta);
            if (normalized == Meta)
                return Result.Failure(CatalogItemErrors.MetaUnchanged());

            Meta = normalized;
            UpdatedAt = DateTime.UtcNow;
            AddDomainEvent(new CatalogItemMetaUpdatedEvent(Id.Value, Code));
            return Result.Success();
        }

        public Result Activate()
        {
            if (IsActive)
                return Result.Failure(CatalogItemErrors.AlreadyActive(Id.Value));

            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
            AddDomainEvent(new CatalogItemActivatedEvent(Id.Value, Code));
            return Result.Success();
        }

        // غیرفعال‌سازی فقط یعنی حذف از انتخاب‌های جدید؛
        // ارجاع‌های CatalogItemId ذخیره‌شده در خطوط طرح/خرید معتبر می‌مانند
        public Result Deactivate()
        {
            if (!IsActive)
                return Result.Failure(CatalogItemErrors.AlreadyDeactivated(Id.Value));

            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
            AddDomainEvent(new CatalogItemDeactivatedEvent(Id.Value, Code));
            return Result.Success();
        }

        private static string? NormalizeMeta(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static Result ValidateCode(string? code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return Result.Failure(CatalogItemErrors.InvalidCode(code));

            if (code.Trim().Length > MaxCodeLength)
                return Result.Failure(CatalogItemErrors.CodeTooLong(code.Trim()));

            return Result.Success();
        }

        private static Result ValidateTitle(string? title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return Result.Failure(CatalogItemErrors.InvalidTitle(title));

            if (title.Trim().Length > MaxTitleLength)
                return Result.Failure(CatalogItemErrors.TitleTooLong(title.Trim()));

            return Result.Success();
        }

        // جفت (قیمت، منبع): قیمت باید مثبت باشد؛ قیمت بدون منبع و منبع بدون قیمت هر دو رد می‌شوند
        private static Result ValidatePricePair(long? refPriceRial, LookupId? priceSourceId)
        {
            if (refPriceRial.HasValue && refPriceRial.Value <= 0)
                return Result.Failure(CatalogItemErrors.InvalidReferencePrice(refPriceRial.Value));

            if (refPriceRial.HasValue && priceSourceId is null)
                return Result.Failure(CatalogItemErrors.ReferencePriceRequiresSource());

            if (!refPriceRial.HasValue && priceSourceId is not null)
                return Result.Failure(CatalogItemErrors.PriceSourceRequiresPrice());

            return Result.Success();
        }
    }
}
