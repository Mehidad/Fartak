using Fartak.Domain.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fartak.Domain.Catalog
{
    // رقم محصول (جدول varieties) — موجودیت فرزندِ Crop؛
    // PK مستقل دارد ولی بدون محصول معنا ندارد و یکتایی کدش (crop_id, code) داخل مرز والد است.
    // فقط از طریق ریشه ساخته و تغییر می‌کند: Crop.AddVariety / RenameVariety / ... ؛
    // رویدادها هم در ریشه صادر می‌شوند — این کلاس فقط وضعیت و اعتبارسنجی خودش را نگه می‌دارد.
    public sealed class Variety : Entity<VarietyId>
    {
        public const int MaxCodeLength = 40;
        public const int MaxTitleLength = 120;

        public string Code { get; private set; }    // یکتا در محدوده محصول (unique crop_id, code)
        public string Title { get; private set; }
        public bool IsActive { get; private set; }
        public string? Meta { get; private set; }   // طول فصل، عملکرد حداقلی، هکتولیتر مرجع و ... (jsonb آزاد)

        private Variety() { } // برای EF Core

        private Variety(VarietyId id, string code, string title, string? meta)
        {
            Id = id;
            Code = code;
            Title = title;
            Meta = meta;
            IsActive = true;
        }

        // فقط ریشه تجمع صدا می‌زند (پیش‌اعتبارسنجی در Crop.AddVariety انجام شده است)
        internal static Variety Create(VarietyId id, string code, string title, string? meta)
        {
            if (id is null || string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(title))
                throw new DomainValidationException(
                    "Crop.VarietyInvalid",
                    "شناسه، کد یا عنوان رقم محصول ارسال نشده است.");

            return new Variety(id, code, title, string.IsNullOrWhiteSpace(meta) ? null : meta.Trim());
        }

        internal Result Rename(string newTitle)
        {
            if (string.IsNullOrWhiteSpace(newTitle))
                return Result.Failure(CropErrors.VarietyInvalidTitle(newTitle));

            var trimmed = newTitle.Trim();
            if (trimmed.Length > MaxTitleLength)
                return Result.Failure(CropErrors.VarietyTitleTooLong(trimmed));

            if (trimmed == Title)
                return Result.Failure(CropErrors.VarietyTitleUnchanged());

            Title = trimmed;
            return Result.Success();
        }

        internal Result SetMeta(string? newMeta)
        {
            var normalized = string.IsNullOrWhiteSpace(newMeta) ? null : newMeta.Trim();
            if (normalized == Meta)
                return Result.Failure(CropErrors.VarietyMetaUnchanged());

            Meta = normalized;
            return Result.Success();
        }

        internal Result Activate()
        {
            if (IsActive)
                return Result.Failure(CropErrors.VarietyAlreadyActive());

            IsActive = true;
            return Result.Success();
        }

        internal Result Deactivate()
        {
            if (!IsActive)
                return Result.Failure(CropErrors.VarietyAlreadyDeactivated());

            IsActive = false;
            return Result.Success();
        }
    }
}
