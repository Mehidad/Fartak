using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Supplier
{
    // خطاهای تجمع تأمین‌کننده
    public static class SupplierErrors
    {
        public static readonly Error NullReference = Error.Create(
            "Supplier.NullReference",
            "مقادیر اجباری تأمین‌کننده ارسال نشده است.");

        public static readonly Error TitleNotSpecified = Error.Create(
            "Supplier.TitleNotSpecified",
            "عنوان تأمین‌کننده باید مشخص باشد.");

        public static Error TitleTooLong(int length)
            => Error.Create(
                code: "Supplier.TitleTooLong",
                message: "عنوان تأمین‌کننده نمی‌تواند بیش از ۱۵۰ نویسه باشد.",
                details: new Dictionary<string, object>
                {
                    ["Length"] = length,
                    ["MaxLength"] = Supplier.MaxTitleLength
                });

        public static Error PhoneTooLong(int length)
            => Error.Create(
                code: "Supplier.PhoneTooLong",
                message: "شماره تماس تأمین‌کننده نمی‌تواند بیش از ۲۰ نویسه باشد.",
                details: new Dictionary<string, object>
                {
                    ["Length"] = length,
                    ["MaxLength"] = Supplier.MaxPhoneLength
                });

        public static Error ContactNameTooLong(int length)
            => Error.Create(
                code: "Supplier.ContactNameTooLong",
                message: "نام رابط تأمین‌کننده نمی‌تواند بیش از ۱۲۰ نویسه باشد.",
                details: new Dictionary<string, object>
                {
                    ["Length"] = length,
                    ["MaxLength"] = Supplier.MaxContactNameLength
                });

        public static Error CategoryTooLong(int length)
            => Error.Create(
                code: "Supplier.CategoryTooLong",
                message: "دستهٔ تأمین‌کننده نمی‌تواند بیش از ۸۰ نویسه باشد.",
                details: new Dictionary<string, object>
                {
                    ["Length"] = length,
                    ["MaxLength"] = Supplier.MaxCategoryLength
                });

        public static readonly Error ContactUnchanged = Error.Create(
            "Supplier.ContactUnchanged",
            "اطلاعات تماس جدید با اطلاعات فعلی یکسان است.");

        public static readonly Error CategoryUnchanged = Error.Create(
            "Supplier.CategoryUnchanged",
            "دستهٔ جدید با دستهٔ فعلی یکسان است.");

        public static readonly Error MetaUnchanged = Error.Create(
            "Supplier.MetaUnchanged",
            "مقدار جدید Meta با مقدار فعلی یکسان است.");

        public static readonly Error AlreadyActive = Error.Create(
            "Supplier.AlreadyActive",
            "این تأمین‌کننده قبلاً فعال است.");

        public static readonly Error AlreadyDeactivated = Error.Create(
            "Supplier.AlreadyDeactivated",
            "این تأمین‌کننده قبلاً غیرفعال شده است.");
    }
}
