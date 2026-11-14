using Fartak.Domain.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fartak.Domain.Catalog
{
    // محصول کاتالوگ (جدول crops) + رقم‌های آن (جدول varieties) — یک تجمع؛
    // ریشه: Crop | فرزند: Variety (هر کاری با رقم فقط از طریق ریشه)
    //
    // قواعد:
    //   - کد محصول و کد رقم پس از ایجاد immutable هستند (کلید معنایی یکتا؛ unique index در ERD)
    //   - یکتایی رقم: جفت (crop_id, code) — داخل مرز همین تجمع کنترل می‌شود
    //   - حذف فیزیکی رقم وجود ندارد؛ فقط فعال/غیرفعال — چون رقم‌ها توسط جداول دیگر
    //     (خطوط طرح، خطوط خرید، ...) ارجاع می‌شوند؛ کد رقمِ غیرفعال هم رزرو می‌ماند
    //   - افزودن رقم فقط در حالت فعالِ محصول مجاز است
    //   - غیرفعال‌کردن محصول رقم‌ها را غیرفعال نمی‌کند (وضعیت‌ها مستقل‌اند؛
    //     انتخاب محصول+رقم در لایه Application با چک هر دو انجام می‌شود)
    public class Crop : AggregateRoot<CropId>
    {
        public const int MaxCodeLength = 40;
        public const int MaxTitleLength = 120;

        public string Code { get; private set; }    // یکتا (unique) — مثل wheat / barley / rice
        public string Title { get; private set; }   // عنوان نمایشی مثل «گندم»
        public bool IsActive { get; private set; }
        public string? Meta { get; private set; }   // jsonb آزاد (اعتبارسنجی ساختار در لایه Application)

        private readonly List<Variety> _varieties = new();
        public IReadOnlyCollection<Variety> Varieties => _varieties.AsReadOnly();

        private Crop() { } // برای EF Core

        private Crop(CropId id, string code, string title)
        {
            Id = id;
            Code = code;
            Title = title;
            IsActive = true;
        }

        public static Result<Crop> Create(
            CropId id,
            string code,
            string title,
            string? meta = null)
        {
            if (id is null)
                return Result.Failure<Crop>(CropErrors.NullReferenceError());

            var codeValidation = ValidateCode(code);
            if (codeValidation.IsFailure)
                return Result.Failure<Crop>(codeValidation.Error);

            var titleValidation = ValidateTitle(title);
            if (titleValidation.IsFailure)
                return Result.Failure<Crop>(titleValidation.Error);

            var crop = new Crop(id, code.Trim(), title.Trim());
            crop.Meta = string.IsNullOrWhiteSpace(meta) ? null : meta.Trim();

            crop.AddDomainEvent(new CropCreatedEvent(id.Value, crop.Code, crop.Title));
            return Result<Crop>.Success(crop);
        }

        // تغییر عنوان نمایشی محصول (فرم ویرایش) — کد محصول تغییر نمی‌کند
        public Result Rename(string newTitle)
        {
            var validation = ValidateTitle(newTitle);
            if (validation.IsFailure)
                return validation;

            var trimmed = newTitle.Trim();
            if (trimmed == Title)
                return Result.Failure(CropErrors.TitleUnchanged());

            var oldTitle = Title;
            Title = trimmed;
            AddDomainEvent(new CropRenamedEvent(Id.Value, oldTitle, Title));
            return Result.Success();
        }

        // به‌روزرسانی Meta (jsonb آزاد) — با null قابل پاک‌سازی است
        public Result SetMeta(string? meta)
        {
            var normalized = string.IsNullOrWhiteSpace(meta) ? null : meta.Trim();
            if (normalized == Meta)
                return Result.Failure(CropErrors.MetaUnchanged());

            Meta = normalized;
            AddDomainEvent(new CropMetaUpdatedEvent(Id.Value));
            return Result.Success();
        }

        public Result Activate()
        {
            if (IsActive)
                return Result.Failure(CropErrors.AlreadyActive(Id.Value));

            IsActive = true;
            AddDomainEvent(new CropActivatedEvent(Id.Value));
            return Result.Success();
        }

        // رقم‌ها دست‌نخورده می‌مانند — فقط انتخاب محصول+رقم جدید در Application بسته می‌شود
        public Result Deactivate()
        {
            if (!IsActive)
                return Result.Failure(CropErrors.AlreadyDeactivated(Id.Value));

            IsActive = false;
            AddDomainEvent(new CropDeactivatedEvent(Id.Value));
            return Result.Success();
        }

        // افزودن رقم به محصول (فرم مدیریتی) — کد رقم باید در محدوده این محصول یکتا باشد
        public Result AddVariety(
            VarietyId varietyId,
            string code,
            string title,
            string? meta = null)
        {
            if (varietyId is null)
                return Result.Failure(CropErrors.NullReferenceError());

            if (!IsActive)
                return Result.Failure(CropErrors.CropNotActive(Id.Value));

            if (_varieties.Any(v => v.Id == varietyId))
                return Result.Failure(CropErrors.VarietyAlreadyExists(Id.Value, varietyId.Value));

            var codeValidation = ValidateVarietyCode(code);
            if (codeValidation.IsFailure)
                return codeValidation;

            var titleValidation = ValidateVarietyTitle(title);
            if (titleValidation.IsFailure)
                return titleValidation;

            var trimmedCode = code.Trim();
            if (_varieties.Any(v => v.Code == trimmedCode))
                return Result.Failure(CropErrors.VarietyCodeDuplicated(Id.Value, trimmedCode));

            var variety = Variety.Create(varietyId, trimmedCode, title.Trim(), meta);
            _varieties.Add(variety);

            AddDomainEvent(new CropVarietyAddedEvent(Id.Value, varietyId.Value, variety.Code, variety.Title));
            return Result.Success();
        }

        // تغییر عنوان نمایشی رقم — کد رقم تغییر نمی‌کند
        public Result RenameVariety(VarietyId varietyId, string newTitle)
        {
            var variety = GetVariety(varietyId);
            if (variety is null)
                return Result.Failure(CropErrors.VarietyNotFound(Id.Value, varietyId?.Value ?? 0));

            var validation = variety.Rename(newTitle);
            if (validation.IsFailure)
                return validation;

            AddDomainEvent(new CropVarietyRenamedEvent(Id.Value, varietyId.Value, variety.Code, newTitle.Trim()));
            return Result.Success();
        }

        // به‌روزرسانی Meta رقم — با null قابل پاک‌سازی است
        public Result SetVarietyMeta(VarietyId varietyId, string? meta)
        {
            var variety = GetVariety(varietyId);
            if (variety is null)
                return Result.Failure(CropErrors.VarietyNotFound(Id.Value, varietyId?.Value ?? 0));

            var validation = variety.SetMeta(meta);
            if (validation.IsFailure)
                return validation;

            AddDomainEvent(new CropVarietyMetaUpdatedEvent(Id.Value, varietyId.Value));
            return Result.Success();
        }

        public Result ActivateVariety(VarietyId varietyId)
        {
            var variety = GetVariety(varietyId);
            if (variety is null)
                return Result.Failure(CropErrors.VarietyNotFound(Id.Value, varietyId?.Value ?? 0));

            var validation = variety.Activate();
            if (validation.IsFailure)
                return validation;

            AddDomainEvent(new CropVarietyActivatedEvent(Id.Value, varietyId.Value));
            return Result.Success();
        }

        // غیرفعال‌سازی رقم — از کامبوهای جدید حذف می‌شود ولی ارجاع‌های موجود معتبر می‌مانند
        public Result DeactivateVariety(VarietyId varietyId)
        {
            var variety = GetVariety(varietyId);
            if (variety is null)
                return Result.Failure(CropErrors.VarietyNotFound(Id.Value, varietyId?.Value ?? 0));

            var validation = variety.Deactivate();
            if (validation.IsFailure)
                return validation;

            AddDomainEvent(new CropVarietyDeactivatedEvent(Id.Value, varietyId.Value));
            return Result.Success();
        }

        private Variety? GetVariety(VarietyId varietyId)
            => varietyId is null ? null : _varieties.FirstOrDefault(v => v.Id == varietyId);

        private static Result ValidateCode(string? code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return Result.Failure(CropErrors.InvalidCode(code));

            if (code.Trim().Length > MaxCodeLength)
                return Result.Failure(CropErrors.CodeTooLong(code.Trim()));

            return Result.Success();
        }

        private static Result ValidateTitle(string? title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return Result.Failure(CropErrors.InvalidTitle(title));

            if (title.Trim().Length > MaxTitleLength)
                return Result.Failure(CropErrors.TitleTooLong(title.Trim()));

            return Result.Success();
        }

        private static Result ValidateVarietyCode(string? code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return Result.Failure(CropErrors.VarietyInvalidCode(code));

            if (code.Trim().Length > MaxCodeLength)
                return Result.Failure(CropErrors.VarietyCodeTooLong(code.Trim()));

            return Result.Success();
        }

        private static Result ValidateVarietyTitle(string? title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return Result.Failure(CropErrors.VarietyInvalidTitle(title));

            if (title.Trim().Length > MaxTitleLength)
                return Result.Failure(CropErrors.VarietyTitleTooLong(title.Trim()));

            return Result.Success();
        }
    }
}
