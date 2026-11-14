using Fartak.Domain.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fartak.Domain.Lookup
{
    public static class LookupErrors
    {
        public static Error NullReferenceError()
            => Error.Create(
                code: "Lookup.NullReference",
                message: "مقادیر اجباری فهرست پایه ارسال نشده است.");

        public static Error InvalidDomain(string? domain)
            => Error.Create(
                code: "Lookup.InvalidDomain",
                message: "نام دامنه فهرست نامعتبر است و نمی‌تواند خالی باشد.",
                details: new Dictionary<string, object>
                {
                    ["Domain"] = domain ?? string.Empty
                });

        public static Error DomainTooLong(string domain)
            => Error.Create(
                code: "Lookup.DomainTooLong",
                message: "نام دامنه فهرست نباید بیش از ۵۰ کاراکتر باشد.",
                details: new Dictionary<string, object>
                {
                    ["Domain"] = domain
                });

        public static Error InvalidCode(string? code)
            => Error.Create(
                code: "Lookup.InvalidCode",
                message: "کد مقدار فهرست نامعتبر است و نمی‌تواند خالی باشد.",
                details: new Dictionary<string, object>
                {
                    ["Code"] = code ?? string.Empty
                });

        public static Error CodeTooLong(string code)
            => Error.Create(
                code: "Lookup.CodeTooLong",
                message: "کد مقدار فهرست نباید بیش از ۵۰ کاراکتر باشد.",
                details: new Dictionary<string, object>
                {
                    ["Code"] = code
                });

        public static Error InvalidTitle(string? title)
            => Error.Create(
                code: "Lookup.InvalidTitle",
                message: "عنوان مقدار فهرست نامعتبر است و نمی‌تواند خالی باشد.",
                details: new Dictionary<string, object>
                {
                    ["Title"] = title ?? string.Empty
                });

        public static Error TitleTooLong(string title)
            => Error.Create(
                code: "Lookup.TitleTooLong",
                message: "عنوان مقدار فهرست نباید بیش از ۱۲۰ کاراکتر باشد.",
                details: new Dictionary<string, object>
                {
                    ["Title"] = title
                });

        public static Error TitleUnchanged()
            => Error.Create(
                code: "Lookup.TitleUnchanged",
                message: "عنوان جدید همان عنوان فعلی این مقدار فهرست است.");

        public static Error MetaUnchanged()
            => Error.Create(
                code: "Lookup.MetaUnchanged",
                message: "مقدار جدید Meta همان مقدار فعلی این ردیف فهرست است.");

        public static Error SortOrderOutOfRange(int sortOrder)
            => Error.Create(
                code: "Lookup.SortOrderOutOfRange",
                message: "ترتیب نمایش باید عددی بین ۰ تا ۳۲۷۶۷ باشد.",
                details: new Dictionary<string, object>
                {
                    ["SortOrder"] = sortOrder
                });

        public static Error SortOrderUnchanged()
            => Error.Create(
                code: "Lookup.SortOrderUnchanged",
                message: "ترتیب نمایش جدید همان ترتیب فعلی این ردیف فهرست است.");

        public static Error AlreadyActive(long lookupId)
            => Error.Create(
                code: "Lookup.AlreadyActive",
                message: "این مقدار فهرست از قبل فعال است.",
                details: new Dictionary<string, object>
                {
                    ["LookupId"] = lookupId
                });

        public static Error AlreadyDeactivated(long lookupId)
            => Error.Create(
                code: "Lookup.AlreadyDeactivated",
                message: "این مقدار فهرست از قبل غیرفعال است.",
                details: new Dictionary<string, object>
                {
                    ["LookupId"] = lookupId
                });
    }
}
