using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Checklist
{
    public static class ChecklistErrors
    {
        public static Error NullReferenceError()
            => Error.Create(
                code: "Checklist.NullReference",
                message: "مقادیر اجباری قالب چک‌لیست ارسال نشده است.");

        public static Error InvalidTitle(string? title)
            => Error.Create(
                code: "Checklist.InvalidTitle",
                message: "عنوان قالب چک‌لیست نامعتبر است و نمی‌تواند خالی باشد.",
                details: new Dictionary<string, object>
                {
                    ["Title"] = title ?? string.Empty
                });

        public static Error TitleTooLong(string title)
            => Error.Create(
                code: "Checklist.TitleTooLong",
                message: "عنوان قالب چک‌لیست نباید بیش از ۱۵۰ کاراکتر باشد.",
                details: new Dictionary<string, object>
                {
                    ["Title"] = title
                });

        public static Error TitleUnchanged()
            => Error.Create(
                code: "Checklist.TitleUnchanged",
                message: "عنوان جدید همان عنوان فعلی قالب چک‌لیست است.");

        public static Error MetaUnchanged()
            => Error.Create(
                code: "Checklist.MetaUnchanged",
                message: "محتوای Meta ارسالی همان مقدار فعلی قالب چک‌لیست است.");

        public static Error TemplateNotActive()
            => Error.Create(
                code: "Checklist.TemplateNotActive",
                message: "به قالب چک‌لیست غیرفعال نمی‌توان آیتم افزود.");

        public static Error AlreadyActive(long checklistTemplateId)
            => Error.Create(
                code: "Checklist.AlreadyActive",
                message: "قالب چک‌لیست قبلاً فعال است.",
                details: new Dictionary<string, object>
                {
                    ["ChecklistTemplateId"] = checklistTemplateId
                });

        public static Error AlreadyDeactivated(long checklistTemplateId)
            => Error.Create(
                code: "Checklist.AlreadyDeactivated",
                message: "قالب چک‌لیست قبلاً غیرفعال شده است.",
                details: new Dictionary<string, object>
                {
                    ["ChecklistTemplateId"] = checklistTemplateId
                });

        public static Error ItemNullReference()
            => Error.Create(
                code: "Checklist.ItemNullReference",
                message: "شناسه آیتم یا نوع پاسخ آیتم ارسال نشده است.");

        public static Error ItemNotFound(long itemId)
            => Error.Create(
                code: "Checklist.ItemNotFound",
                message: "آیتم موردنظر در این قالب چک‌لیست یافت نشد.",
                details: new Dictionary<string, object>
                {
                    ["ItemId"] = itemId
                });

        public static Error ItemTitleInvalid(string? title)
            => Error.Create(
                code: "Checklist.ItemTitleInvalid",
                message: "عنوان آیتم چک‌لیست نامعتبر است و نمی‌تواند خالی باشد.",
                details: new Dictionary<string, object>
                {
                    ["Title"] = title ?? string.Empty
                });

        public static Error ItemTitleTooLong(string title)
            => Error.Create(
                code: "Checklist.ItemTitleTooLong",
                message: "عنوان آیتم چک‌لیست نباید بیش از ۱۵۰ کاراکتر باشد.",
                details: new Dictionary<string, object>
                {
                    ["Title"] = title
                });

        public static Error ItemTitleUnchanged()
            => Error.Create(
                code: "Checklist.ItemTitleUnchanged",
                message: "عنوان جدید همان عنوان فعلی آیتم چک‌لیست است.");

        public static Error ItemTitleDuplicated(string title)
            => Error.Create(
                code: "Checklist.ItemTitleDuplicated",
                message: "در این قالب چک‌لیست قبلاً آیتمی با همین عنوان ثبت شده است.",
                details: new Dictionary<string, object>
                {
                    ["Title"] = title
                });

        public static Error SortOrderOutOfRange(int sortOrder)
            => Error.Create(
                code: "Checklist.SortOrderOutOfRange",
                message: "ترتیب نمایش باید عددی بین ۰ تا ۳۲۷۶۷ باشد.",
                details: new Dictionary<string, object>
                {
                    ["SortOrder"] = sortOrder
                });

        public static Error ItemSortOrderUnchanged()
            => Error.Create(
                code: "Checklist.ItemSortOrderUnchanged",
                message: "ترتیب نمایش جدید همان ترتیب فعلی آیتم است.");

        public static Error ItemAnswerTypeUnchanged()
            => Error.Create(
                code: "Checklist.ItemAnswerTypeUnchanged",
                message: "نوع پاسخ ارسالی همان نوع پاسخ فعلی آیتم است.");

        public static Error ItemUnitUnchanged()
            => Error.Create(
                code: "Checklist.ItemUnitUnchanged",
                message: "واحد ارسالی همان واحد فعلی آیتم چک‌لیست است.");

        public static Error ItemRequirementUnchanged()
            => Error.Create(
                code: "Checklist.ItemRequirementUnchanged",
                message: "اجباری/اختیاری‌بودن آیتم قبلاً با همین مقدار تنظیم شده است.");
    }
}
