using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fartak.Domain.CostCategory
{
    public static class CostCategoryErrors
    {
        public static Error NullReferenceError()
            => Error.Create(
                code: "CostCategory.NullReference",
                message: "مقادیر اجباری سرفصل هزینه ارسال نشده است.");

        public static Error InvalidCode(string? code)
            => Error.Create(
                code: "CostCategory.InvalidCode",
                message: "کد سرفصل هزینه نامعتبر است و نمی‌تواند خالی باشد.",
                details: new Dictionary<string, object>
                {
                    ["Code"] = code ?? string.Empty
                });

        public static Error CodeTooLong(string code)
            => Error.Create(
                code: "CostCategory.CodeTooLong",
                message: "کد سرفصل هزینه نباید بیش از ۴۰ کاراکتر باشد.",
                details: new Dictionary<string, object>
                {
                    ["Code"] = code
                });

        public static Error InvalidTitle(string? title)
            => Error.Create(
                code: "CostCategory.InvalidTitle",
                message: "عنوان سرفصل هزینه نامعتبر است و نمی‌تواند خالی باشد.",
                details: new Dictionary<string, object>
                {
                    ["Title"] = title ?? string.Empty
                });

        public static Error TitleTooLong(string title)
            => Error.Create(
                code: "CostCategory.TitleTooLong",
                message: "عنوان سرفصل هزینه نباید بیش از ۱۲۰ کاراکتر باشد.",
                details: new Dictionary<string, object>
                {
                    ["Title"] = title
                });

        public static Error TitleUnchanged()
            => Error.Create(
                code: "CostCategory.TitleUnchanged",
                message: "عنوان جدید همان عنوان فعلی سرفصل هزینه است.");

        public static Error SelfParent()
            => Error.Create(
                code: "CostCategory.SelfParent",
                message: "سرفصل هزینه نمی‌تواند والد خودش باشد.");

        public static Error ParentStateNotProvided()
            => Error.Create(
                code: "CostCategory.ParentStateNotProvided",
                message: "وضعیت فعال/غیرفعال سرفصل والد از سوی لایه Application تأیید نشده است.");

        public static Error InactiveParent()
            => Error.Create(
                code: "CostCategory.InactiveParent",
                message: "سرفصل والد غیرفعال است؛ تعریف یا انتقال سرفصل زیر آن مجاز نیست.");

        public static Error ParentUnchanged()
            => Error.Create(
                code: "CostCategory.ParentUnchanged",
                message: "والد جدید همان والد فعلی سرفصل هزینه است.");

        public static Error SortOrderOutOfRange(int sortOrder)
            => Error.Create(
                code: "CostCategory.SortOrderOutOfRange",
                message: "ترتیب نمایش باید عددی بین ۰ تا ۳۲۷۶۷ باشد.",
                details: new Dictionary<string, object>
                {
                    ["SortOrder"] = sortOrder
                });

        public static Error SortOrderUnchanged()
            => Error.Create(
                code: "CostCategory.SortOrderUnchanged",
                message: "ترتیب نمایش جدید همان ترتیب فعلی سرفصل هزینه است.");

        public static Error AlreadyActive(long costCategoryId)
            => Error.Create(
                code: "CostCategory.AlreadyActive",
                message: "سرفصل هزینه قبلاً فعال است.",
                details: new Dictionary<string, object>
                {
                    ["CostCategoryId"] = costCategoryId
                });

        public static Error AlreadyDeactivated(long costCategoryId)
            => Error.Create(
                code: "CostCategory.AlreadyDeactivated",
                message: "سرفصل هزینه قبلاً غیرفعال شده است.",
                details: new Dictionary<string, object>
                {
                    ["CostCategoryId"] = costCategoryId
                });
    }
}
