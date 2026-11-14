using Fartak.Domain.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fartak.Domain.Catalog
{
    public static class CropErrors
    {
        public static Error NullReferenceError()
            => Error.Create(
                code: "Crop.NullReference",
                message: "مقادیر اجباری محصول ارسال نشده است.");

        public static Error InvalidCode(string? code)
            => Error.Create(
                code: "Crop.InvalidCode",
                message: "کد محصول نامعتبر است و نمی‌تواند خالی باشد.",
                details: new Dictionary<string, object>
                {
                    ["Code"] = code ?? string.Empty
                });

        public static Error CodeTooLong(string code)
            => Error.Create(
                code: "Crop.CodeTooLong",
                message: "کد محصول نباید بیش از ۴۰ کاراکتر باشد.",
                details: new Dictionary<string, object>
                {
                    ["Code"] = code
                });

        public static Error InvalidTitle(string? title)
            => Error.Create(
                code: "Crop.InvalidTitle",
                message: "عنوان محصول نامعتبر است و نمی‌تواند خالی باشد.",
                details: new Dictionary<string, object>
                {
                    ["Title"] = title ?? string.Empty
                });

        public static Error TitleTooLong(string title)
            => Error.Create(
                code: "Crop.TitleTooLong",
                message: "عنوان محصول نباید بیش از ۱۲۰ کاراکتر باشد.",
                details: new Dictionary<string, object>
                {
                    ["Title"] = title
                });

        public static Error TitleUnchanged()
            => Error.Create(
                code: "Crop.TitleUnchanged",
                message: "عنوان جدید همان عنوان فعلی محصول است.");

        public static Error MetaUnchanged()
            => Error.Create(
                code: "Crop.MetaUnchanged",
                message: "مقدار جدید Meta همان مقدار فعلی محصول است.");

        public static Error AlreadyActive(long cropId)
            => Error.Create(
                code: "Crop.AlreadyActive",
                message: "این محصول از قبل فعال است.",
                details: new Dictionary<string, object>
                {
                    ["CropId"] = cropId
                });

        public static Error AlreadyDeactivated(long cropId)
            => Error.Create(
                code: "Crop.AlreadyDeactivated",
                message: "این محصول از قبل غیرفعال است.",
                details: new Dictionary<string, object>
                {
                    ["CropId"] = cropId
                });

        public static Error CropNotActive(long cropId)
            => Error.Create(
                code: "Crop.NotActive",
                message: "برای محصول غیرفعال نمی‌توان رقم افزود؛ ابتدا محصول را فعال کنید.",
                details: new Dictionary<string, object>
                {
                    ["CropId"] = cropId
                });

        public static Error VarietyNotFound(long cropId, long varietyId)
            => Error.Create(
                code: "Crop.VarietyNotFound",
                message: "رقم موردنظر برای این محصول یافت نشد.",
                details: new Dictionary<string, object>
                {
                    ["CropId"] = cropId,
                    ["VarietyId"] = varietyId
                });

        public static Error VarietyAlreadyExists(long cropId, long varietyId)
            => Error.Create(
                code: "Crop.VarietyAlreadyExists",
                message: "این رقم قبلاً به محصول اضافه شده است.",
                details: new Dictionary<string, object>
                {
                    ["CropId"] = cropId,
                    ["VarietyId"] = varietyId
                });

        public static Error VarietyCodeDuplicated(long cropId, string code)
            => Error.Create(
                code: "Crop.VarietyCodeDuplicated",
                message: "کد رقم در محدوده این محصول تکراری است.",
                details: new Dictionary<string, object>
                {
                    ["CropId"] = cropId,
                    ["Code"] = code
                });

        public static Error VarietyInvalidCode(string? code)
            => Error.Create(
                code: "Crop.VarietyInvalidCode",
                message: "کد رقم نامعتبر است و نمی‌تواند خالی باشد.",
                details: new Dictionary<string, object>
                {
                    ["Code"] = code ?? string.Empty
                });

        public static Error VarietyCodeTooLong(string code)
            => Error.Create(
                code: "Crop.VarietyCodeTooLong",
                message: "کد رقم نباید بیش از ۴۰ کاراکتر باشد.",
                details: new Dictionary<string, object>
                {
                    ["Code"] = code
                });

        public static Error VarietyInvalidTitle(string? title)
            => Error.Create(
                code: "Crop.VarietyInvalidTitle",
                message: "عنوان رقم نامعتبر است و نمی‌تواند خالی باشد.",
                details: new Dictionary<string, object>
                {
                    ["Title"] = title ?? string.Empty
                });

        public static Error VarietyTitleTooLong(string title)
            => Error.Create(
                code: "Crop.VarietyTitleTooLong",
                message: "عنوان رقم نباید بیش از ۱۲۰ کاراکتر باشد.",
                details: new Dictionary<string, object>
                {
                    ["Title"] = title
                });

        public static Error VarietyTitleUnchanged()
            => Error.Create(
                code: "Crop.VarietyTitleUnchanged",
                message: "عنوان جدید همان عنوان فعلی رقم است.");

        public static Error VarietyMetaUnchanged()
            => Error.Create(
                code: "Crop.VarietyMetaUnchanged",
                message: "مقدار جدید Meta همان مقدار فعلی رقم است.");

        public static Error VarietyAlreadyActive()
            => Error.Create(
                code: "Crop.VarietyAlreadyActive",
                message: "این رقم از قبل فعال است.");

        public static Error VarietyAlreadyDeactivated()
            => Error.Create(
                code: "Crop.VarietyAlreadyDeactivated",
                message: "این رقم از قبل غیرفعال است.");
    }
}
