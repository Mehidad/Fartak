using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Catalog
{
    public static class CatalogItemErrors
    {
        public static Error NullReferenceError()
            => Error.Create(
                code: "CatalogItem.NullReference",
                message: "مقادیر اجباری قلم کاتالوگ ارسال نشده است.");

        public static Error InvalidCode(string? code)
            => Error.Create(
                code: "CatalogItem.InvalidCode",
                message: "کد قلم کاتالوگ نامعتبر است و نمی‌تواند خالی باشد.",
                details: new Dictionary<string, object>
                {
                    ["Code"] = code ?? string.Empty
                });

        public static Error CodeTooLong(string code)
            => Error.Create(
                code: "CatalogItem.CodeTooLong",
                message: "کد قلم کاتالوگ نباید بیش از ۴۰ کاراکتر باشد.",
                details: new Dictionary<string, object>
                {
                    ["Code"] = code
                });

        public static Error InvalidTitle(string? title)
            => Error.Create(
                code: "CatalogItem.InvalidTitle",
                message: "عنوان قلم کاتالوگ نامعتبر است و نمی‌تواند خالی باشد.",
                details: new Dictionary<string, object>
                {
                    ["Title"] = title ?? string.Empty
                });

        public static Error TitleTooLong(string title)
            => Error.Create(
                code: "CatalogItem.TitleTooLong",
                message: "عنوان قلم کاتالوگ نباید بیش از ۱۵۰ کاراکتر باشد.",
                details: new Dictionary<string, object>
                {
                    ["Title"] = title
                });

        public static Error TitleUnchanged()
            => Error.Create(
                code: "CatalogItem.TitleUnchanged",
                message: "عنوان جدید همان عنوان فعلی قلم کاتالوگ است.");

        public static Error InvalidReferencePrice(long price)
            => Error.Create(
                code: "CatalogItem.InvalidReferencePrice",
                message: "قیمت مرجع باید عددی بزرگ‌تر از صفر باشد.",
                details: new Dictionary<string, object>
                {
                    ["RefPriceRial"] = price
                });

        public static Error ReferencePriceRequiresSource()
            => Error.Create(
                code: "CatalogItem.ReferencePriceRequiresSource",
                message: "ثبت قیمت مرجع بدون تعیین منبع قیمت مجاز نیست؛ منبع قیمت (دستی یا سرویس بیرونی) الزامی است.");

        public static Error PriceSourceRequiresPrice()
            => Error.Create(
                code: "CatalogItem.PriceSourceRequiresPrice",
                message: "تعیین منبع قیمت بدون ثبت قیمت مرجع معنا ندارد.");

        public static Error ReferenceUnchanged()
            => Error.Create(
                code: "CatalogItem.ReferenceUnchanged",
                message: "قیمت مرجع و منبع ارسالی همان مقادیر فعلی قلم کاتالوگ است.");

        public static Error UnitUnchanged()
            => Error.Create(
                code: "CatalogItem.UnitUnchanged",
                message: "واحد پیش‌فرض ارسالی همان واحد فعلی قلم کاتالوگ است.");

        public static Error MetaUnchanged()
            => Error.Create(
                code: "CatalogItem.MetaUnchanged",
                message: "محتوای Meta ارسالی همان مقدار فعلی قلم کاتالوگ است.");

        public static Error AlreadyActive(long catalogItemId)
            => Error.Create(
                code: "CatalogItem.AlreadyActive",
                message: "قلم کاتالوگ قبلاً فعال است.",
                details: new Dictionary<string, object>
                {
                    ["CatalogItemId"] = catalogItemId
                });

        public static Error AlreadyDeactivated(long catalogItemId)
            => Error.Create(
                code: "CatalogItem.AlreadyDeactivated",
                message: "قلم کاتالوگ قبلاً غیرفعال شده است.",
                details: new Dictionary<string, object>
                {
                    ["CatalogItemId"] = catalogItemId
                });
    }
}
