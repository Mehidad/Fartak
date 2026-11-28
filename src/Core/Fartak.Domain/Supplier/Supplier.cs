using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;
using Fartak.Domain.Supplier.Events;

namespace Fartak.Domain.Supplier
{
    // تأمین‌کننده (جدول suppliers) — طرف حساب سفارش‌های تجمیعی تأمین نهاده‌ها؛
    // خطوط برنامهٔ چند پرونده به سفارش تجمیعی تبدیل و برای تأمین‌کننده ارسال می‌شود.
    //
    // طبق قاعده timestamp/is_active: اسکیما نه created_at دارد و نه updated_at
    // → تجمع هیچ فیلد زمانی نگه نمی‌دارد؛ در عوض is_active دارد → Activate/Deactivate.
    // meta (jsonb آزاد) — اعتبارسنجی ساختار در لایه Application انجام می‌شود (الگوی CatalogItem).
    public sealed class Supplier : AggregateRoot<SupplierId>
    {
        public const int MaxTitleLength = 150;          // varchar(150) NOT NULL
        public const int MaxPhoneLength = 20;           // varchar(20)
        public const int MaxContactNameLength = 120;    // varchar(120)
        public const int MaxCategoryLength = 80;        // varchar(80)

        public string Title { get; private set; }                // نام تأمین‌کننده
        public string? Phone { get; private set; }               // شماره تماس
        public string? ContactName { get; private set; }         // نام رابط
        public string? Category { get; private set; }            // دسته: کود / سم / بذر / ماشین‌آلات / ...
        public bool IsActive { get; private set; }               // DEFAULT true
        public string? Meta { get; private set; }                // jsonb آزاد (اعتبارسنجی ساختار در لایه Application)

        private Supplier() { } // برای EF Core

        private Supplier(
            SupplierId id,
            string title,
            string? phone,
            string? contactName,
            string? category,
            string? meta)
            : base(id)
        {
            Title = title;
            Phone = phone;
            ContactName = contactName;
            Category = category;
            IsActive = true;
            Meta = meta;
        }

        public static Result<Supplier> Create(
            SupplierId id,
            string title,
            string? phone = null,
            string? contactName = null,
            string? category = null,
            string? meta = null)
        {
            if (id is null)
                return Result.Failure<Supplier>(SupplierErrors.NullReference);

            if (string.IsNullOrWhiteSpace(title))
                return Result.Failure<Supplier>(SupplierErrors.TitleNotSpecified);

            var trimmedTitle = title.Trim();
            if (trimmedTitle.Length > MaxTitleLength)
                return Result.Failure<Supplier>(SupplierErrors.TitleTooLong(trimmedTitle.Length));

            var normalizedPhone = NormalizeText(phone);
            if (normalizedPhone is not null && normalizedPhone.Length > MaxPhoneLength)
                return Result.Failure<Supplier>(SupplierErrors.PhoneTooLong(normalizedPhone.Length));

            var normalizedContactName = NormalizeText(contactName);
            if (normalizedContactName is not null && normalizedContactName.Length > MaxContactNameLength)
                return Result.Failure<Supplier>(SupplierErrors.ContactNameTooLong(normalizedContactName.Length));

            var normalizedCategory = NormalizeText(category);
            if (normalizedCategory is not null && normalizedCategory.Length > MaxCategoryLength)
                return Result.Failure<Supplier>(SupplierErrors.CategoryTooLong(normalizedCategory.Length));

            var supplier = new Supplier(
                id, trimmedTitle, normalizedPhone, normalizedContactName,
                normalizedCategory, NormalizeText(meta));

            supplier.AddDomainEvent(new SupplierCreatedEvent(
                id.Value,
                supplier.Title,
                supplier.Phone,
                supplier.ContactName,
                supplier.Category,
                supplier.Meta));

            return Result.Success(supplier);
        }

        // به‌روزرسانی اطلاعات تماس — تلفن و نام رابط؛ هر دو با null قابل پاک‌سازی‌اند
        public Result UpdateContact(string? phone, string? contactName)
        {
            var normalizedPhone = NormalizeText(phone);
            if (normalizedPhone is not null && normalizedPhone.Length > MaxPhoneLength)
                return Result.Failure(SupplierErrors.PhoneTooLong(normalizedPhone.Length));

            var normalizedContactName = NormalizeText(contactName);
            if (normalizedContactName is not null && normalizedContactName.Length > MaxContactNameLength)
                return Result.Failure(SupplierErrors.ContactNameTooLong(normalizedContactName.Length));

            if (normalizedPhone == Phone && normalizedContactName == ContactName)
                return Result.Failure(SupplierErrors.ContactUnchanged);

            var oldPhone = Phone;
            var oldContactName = ContactName;
            Phone = normalizedPhone;
            ContactName = normalizedContactName;

            AddDomainEvent(new SupplierContactUpdatedEvent(
                Id.Value,
                oldPhone,
                normalizedPhone,
                oldContactName,
                normalizedContactName));

            return Result.Success();
        }

        // تغییر دستهٔ فعالیت تأمین‌کننده
        public Result ChangeCategory(string? category)
        {
            var normalizedCategory = NormalizeText(category);
            if (normalizedCategory is not null && normalizedCategory.Length > MaxCategoryLength)
                return Result.Failure(SupplierErrors.CategoryTooLong(normalizedCategory.Length));

            if (normalizedCategory == Category)
                return Result.Failure(SupplierErrors.CategoryUnchanged);

            var oldCategory = Category;
            Category = normalizedCategory;

            AddDomainEvent(new SupplierCategoryChangedEvent(
                Id.Value,
                oldCategory,
                normalizedCategory));

            return Result.Success();
        }

        // به‌روزرسانی Meta (jsonb آزاد) — با null قابل پاک‌سازی است
        public Result SetMeta(string? meta)
        {
            var normalized = NormalizeText(meta);
            if (normalized == Meta)
                return Result.Failure(SupplierErrors.MetaUnchanged);

            var oldMeta = Meta;
            Meta = normalized;

            AddDomainEvent(new SupplierMetaChangedEvent(
                Id.Value,
                oldMeta,
                normalized));

            return Result.Success();
        }

        public Result Activate()
        {
            if (IsActive)
                return Result.Failure(SupplierErrors.AlreadyActive);

            IsActive = true;
            AddDomainEvent(new SupplierActivatedEvent(Id.Value));
            return Result.Success();
        }

        // غیرفعال‌سازی — تأمین‌کنندهٔ غیرفعال برای سفارش جدید انتخاب نمی‌شود (کنترل در Application)
        public Result Deactivate()
        {
            if (!IsActive)
                return Result.Failure(SupplierErrors.AlreadyDeactivated);

            IsActive = false;
            AddDomainEvent(new SupplierDeactivatedEvent(Id.Value));
            return Result.Success();
        }

        private static string? NormalizeText(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
