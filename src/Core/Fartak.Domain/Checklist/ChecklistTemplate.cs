using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Fartak.Domain.Catalog;
using Fartak.Domain.Common;
using Fartak.Domain.Framework;
using Fartak.Domain.Unit;

namespace Fartak.Domain.Checklist
{
    // قالب چک‌لیست بازدید (جدول checklist_templates) به تفکیک اختیاریِ محصول و نوع بازدید،
    // همراه با آیتم‌های آن (جدول checklist_template_items) به‌عنوان موجودیت‌های فرزند.
    // پاسخ‌های بازدیدهای آینده (field_visit_items) با ChecklistTemplateItemId به آیتم‌ها ارجاع می‌دهند.
    //
    // قاعده تغییرناپذیری گستره قالب: محصول و نوع بازدید پس از ایجاد تغییر نمی‌کنند —
    // تغییر آن‌ها معنای آیتم‌ها و پاسخ‌های ثبت‌شده را می‌شکند؛
    // مسیر درست: غیرفعال‌سازی قالب فعلی و ساخت قالب جدید.
    //
    // قواعد آیتم‌ها: افزودن آیتم فقط در قالب فعال؛ عنوان آیتم درون قالب یکتاست؛
    // واحد اندازه‌گیری آیتم برای پاسخ عددی معنا دارد اما تشخیص عددی‌بودن نوع پاسخ
    // داده lookup است (domain=answer_type) و در دامنه محدود نمی‌شود.
    //
    // حذف فیزیکی آیتم: جدول آیتم‌ها ستون is_active ندارد؛ حذف فقط زمانی مجاز است که
    // پاسخ ثبت‌شده‌ای به آیتم وجود نداشته باشد — بررسی در لایه Application (نیازمند کوئری بین تجمعی).
    //
    // هیچ‌کدام از دو جدول ستون timestamp ندارند — بنابراین این تجمع عمداً بدون CreatedAt/UpdatedAt است.
    public class ChecklistTemplate : AggregateRoot<ChecklistTemplateId>
    {
        public const int MaxTitleLength = 150;    // varchar(150) در ERD (قالب و آیتم)
        public const int MaxSortOrder = 32767;    // سقف smallint مطابق ERD

        public string Title { get; private set; }                 // مثل «گندم — بازدید دوره‌ای»
        public CropId? CropId { get; private set; }               // محصول مرتبط — اختیاری، immutable
        public LookupId? VisitTypeId { get; private set; }        // lookup: visit_type — اختیاری، immutable
        public bool IsActive { get; private set; }
        public string? Meta { get; private set; }                 // jsonb آزاد (اعتبارسنجی ساختار در لایه Application)

        private readonly List<ChecklistTemplateItem> _items = new();

        // آیتم‌های چک‌لیست — موجودیت‌های فرزند
        public IReadOnlyCollection<ChecklistTemplateItem> Items => _items.AsReadOnly();

        private ChecklistTemplate() { } // برای EF Core

        private ChecklistTemplate(ChecklistTemplateId id, string title, CropId? cropId, LookupId? visitTypeId)
        {
            Id = id;
            Title = title;
            CropId = cropId;
            VisitTypeId = visitTypeId;
            IsActive = true;
        }

        public static Result<ChecklistTemplate> Create(
            ChecklistTemplateId id,
            string title,
            CropId? cropId = null,
            LookupId? visitTypeId = null,
            string? meta = null)
        {
            if (id is null)
                return Result.Failure<ChecklistTemplate>(ChecklistErrors.NullReferenceError());

            var titleValidation = ValidateTitle(title);
            if (titleValidation.IsFailure)
                return Result.Failure<ChecklistTemplate>(titleValidation.Error);

            var template = new ChecklistTemplate(id, title.Trim(), cropId, visitTypeId);
            template.Meta = NormalizeMeta(meta);

            template.AddDomainEvent(new ChecklistTemplateCreatedEvent(
                id.Value, template.Title, cropId?.Value, visitTypeId?.Value));
            return Result<ChecklistTemplate>.Success(template);
        }

        // تغییر عنوان نمایشی قالب — محصول و نوع بازدید تغییر نمی‌کنند
        public Result Rename(string newTitle)
        {
            var validation = ValidateTitle(newTitle);
            if (validation.IsFailure)
                return validation;

            var trimmed = newTitle.Trim();
            if (trimmed == Title)
                return Result.Failure(ChecklistErrors.TitleUnchanged());

            var oldTitle = Title;
            Title = trimmed;
            AddDomainEvent(new ChecklistTemplateRenamedEvent(Id.Value, oldTitle, Title));
            return Result.Success();
        }

        // به‌روزرسانی Meta (jsonb آزاد) — با null قابل پاک‌سازی است
        public Result SetMeta(string? meta)
        {
            var normalized = NormalizeMeta(meta);
            if (normalized == Meta)
                return Result.Failure(ChecklistErrors.MetaUnchanged());

            Meta = normalized;
            AddDomainEvent(new ChecklistTemplateMetaUpdatedEvent(Id.Value));
            return Result.Success();
        }

        public Result Activate()
        {
            if (IsActive)
                return Result.Failure(ChecklistErrors.AlreadyActive(Id.Value));

            IsActive = true;
            AddDomainEvent(new ChecklistTemplateActivatedEvent(Id.Value));
            return Result.Success();
        }

        // غیرفعال‌سازی یعنی حذف از انتخاب‌های بازدید جدید؛
        // ارجاع‌های بازدیدهای موجود به قالب و آیتم‌هایش معتبر می‌مانند
        public Result Deactivate()
        {
            if (!IsActive)
                return Result.Failure(ChecklistErrors.AlreadyDeactivated(Id.Value));

            IsActive = false;
            AddDomainEvent(new ChecklistTemplateDeactivatedEvent(Id.Value));
            return Result.Success();
        }

        // افزودن آیتم به چک‌لیست — فقط در قالب فعال؛ عنوان آیتم درون قالب یکتاست
        public Result AddItem(
            ChecklistTemplateItemId itemId,
            string title,
            LookupId answerTypeId,
            UnitId? unitId = null,
            bool isRequired = true,
            int sortOrder = 0)
        {
            if (itemId is null || answerTypeId is null)
                return Result.Failure(ChecklistErrors.ItemNullReference());

            if (!IsActive)
                return Result.Failure(ChecklistErrors.TemplateNotActive());

            var titleValidation = ValidateItemTitle(title);
            if (titleValidation.IsFailure)
                return titleValidation;

            var trimmed = title.Trim();
            if (_items.Any(i => i.Title == trimmed))
                return Result.Failure(ChecklistErrors.ItemTitleDuplicated(trimmed));

            var orderValidation = ValidateSortOrder(sortOrder);
            if (orderValidation.IsFailure)
                return orderValidation;

            var item = ChecklistTemplateItem.Create(itemId, trimmed, answerTypeId, unitId, isRequired, sortOrder);
            _items.Add(item);

            AddDomainEvent(new ChecklistTemplateItemAddedEvent(
                Id.Value, item.Id.Value, item.Title, item.AnswerTypeId.Value, item.IsRequired));
            return Result.Success();
        }

        // حذف فیزیکی آیتم — فقط آیتم بدون پاسخ ثبت‌شده (بررسی نبود پاسخ در لایه Application)
        public Result RemoveItem(ChecklistTemplateItemId itemId)
        {
            var item = FindItem(itemId);
            if (item is null)
                return Result.Failure(ChecklistErrors.ItemNotFound(itemId?.Value ?? 0));

            _items.Remove(item);
            AddDomainEvent(new ChecklistTemplateItemRemovedEvent(Id.Value, item.Id.Value, item.Title));
            return Result.Success();
        }

        // تغییر عنوان آیتم — یکتایی عنوان نسبت به سایر آیتم‌های همان قالب حفظ می‌شود
        public Result RenameItem(ChecklistTemplateItemId itemId, string newTitle)
        {
            var item = FindItem(itemId);
            if (item is null)
                return Result.Failure(ChecklistErrors.ItemNotFound(itemId?.Value ?? 0));

            var titleValidation = ValidateItemTitle(newTitle);
            if (titleValidation.IsFailure)
                return titleValidation;

            var trimmed = newTitle.Trim();
            if (trimmed == item.Title)
                return Result.Failure(ChecklistErrors.ItemTitleUnchanged());

            if (_items.Any(i => i.Id.Value != item.Id.Value && i.Title == trimmed))
                return Result.Failure(ChecklistErrors.ItemTitleDuplicated(trimmed));

            var oldTitle = item.Title;
            item.SetTitle(trimmed);
            AddDomainEvent(new ChecklistTemplateItemRenamedEvent(Id.Value, item.Id.Value, oldTitle, item.Title));
            return Result.Success();
        }

        // تغییر ترتیب نمایش آیتم در فرم بازدید
        public Result SetItemSortOrder(ChecklistTemplateItemId itemId, int newSortOrder)
        {
            var item = FindItem(itemId);
            if (item is null)
                return Result.Failure(ChecklistErrors.ItemNotFound(itemId?.Value ?? 0));

            var orderValidation = ValidateSortOrder(newSortOrder);
            if (orderValidation.IsFailure)
                return orderValidation;

            if (newSortOrder == item.SortOrder)
                return Result.Failure(ChecklistErrors.ItemSortOrderUnchanged());

            var oldOrder = item.SortOrder;
            item.SetSortOrder(newSortOrder);
            AddDomainEvent(new ChecklistTemplateItemSortOrderChangedEvent(Id.Value, item.Id.Value, oldOrder, newSortOrder));
            return Result.Success();
        }

        // تغییر نوع پاسخ آیتم (مقیاس/بله-خیر/عددی/متنی) —
        // محدودسازی تغییر برای آیتمِ دارای پاسخ ثبت‌شده در لایه Application بررسی می‌شود
        public Result SetItemAnswerType(ChecklistTemplateItemId itemId, LookupId newAnswerTypeId)
        {
            var item = FindItem(itemId);
            if (item is null)
                return Result.Failure(ChecklistErrors.ItemNotFound(itemId?.Value ?? 0));

            if (newAnswerTypeId is null)
                return Result.Failure(ChecklistErrors.ItemNullReference());

            if (newAnswerTypeId.Value == item.AnswerTypeId.Value)
                return Result.Failure(ChecklistErrors.ItemAnswerTypeUnchanged());

            var oldAnswerTypeId = item.AnswerTypeId;
            item.SetAnswerType(newAnswerTypeId);
            AddDomainEvent(new ChecklistTemplateItemAnswerTypeChangedEvent(
                Id.Value, item.Id.Value, oldAnswerTypeId.Value, newAnswerTypeId.Value));
            return Result.Success();
        }

        // تعیین/تغییر/پاک‌کردن واحد آیتم — برای پاسخ عددی (مثل تراکم بوته بر واحد m2)؛ تهی یعنی بدون واحد
        public Result SetItemUnit(ChecklistTemplateItemId itemId, UnitId? newUnitId)
        {
            var item = FindItem(itemId);
            if (item is null)
                return Result.Failure(ChecklistErrors.ItemNotFound(itemId?.Value ?? 0));

            if (newUnitId?.Value == item.UnitId?.Value)
                return Result.Failure(ChecklistErrors.ItemUnitUnchanged());

            var oldUnitId = item.UnitId;
            item.SetUnit(newUnitId);
            AddDomainEvent(new ChecklistTemplateItemUnitChangedEvent(
                Id.Value, item.Id.Value, oldUnitId?.Value, newUnitId?.Value));
            return Result.Success();
        }

        // تغییر اجباری/اختیاری‌بودن پاسخ آیتم در فرم بازدید
        public Result SetItemRequirement(ChecklistTemplateItemId itemId, bool isRequired)
        {
            var item = FindItem(itemId);
            if (item is null)
                return Result.Failure(ChecklistErrors.ItemNotFound(itemId?.Value ?? 0));

            if (isRequired == item.IsRequired)
                return Result.Failure(ChecklistErrors.ItemRequirementUnchanged());

            item.SetRequirement(isRequired);
            AddDomainEvent(new ChecklistTemplateItemRequirementChangedEvent(Id.Value, item.Id.Value, isRequired));
            return Result.Success();
        }

        private ChecklistTemplateItem? FindItem(ChecklistTemplateItemId? itemId)
            => itemId is null
                ? null
                : _items.FirstOrDefault(i => i.Id.Value == itemId.Value);

        private static Result ValidateTitle(string? title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return Result.Failure(ChecklistErrors.InvalidTitle(title));

            if (title.Trim().Length > MaxTitleLength)
                return Result.Failure(ChecklistErrors.TitleTooLong(title.Trim()));

            return Result.Success();
        }

        private static Result ValidateItemTitle(string? title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return Result.Failure(ChecklistErrors.ItemTitleInvalid(title));

            if (title.Trim().Length > MaxTitleLength)
                return Result.Failure(ChecklistErrors.ItemTitleTooLong(title.Trim()));

            return Result.Success();
        }

        private static Result ValidateSortOrder(int sortOrder)
        {
            if (sortOrder < 0 || sortOrder > MaxSortOrder)
                return Result.Failure(ChecklistErrors.SortOrderOutOfRange(sortOrder));

            return Result.Success();
        }

        private static string? NormalizeMeta(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
