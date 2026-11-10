using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Region
{
    public static class RegionErrors
    {
        public static Error NullReferenceError()
            => Error.Create(
                code: "Region.NullReference",
                message: "مقادیر اجباری منطقه ارسال نشده است.");

        public static Error InvalidName(string? name)
            => Error.Create(
                code: "Region.InvalidName",
                message: "نام منطقه نامعتبر است.",
                details: new Dictionary<string, object>
                {
                    ["Name"] = name ?? string.Empty
                });

        public static Error NameTooLong(string name)
            => Error.Create(
                code: "Region.NameTooLong",
                message: "نام منطقه نباید بیش از ۱۲۰ کاراکتر باشد.",
                details: new Dictionary<string, object>
                {
                    ["Name"] = name
                });

        public static Error ProvinceCannotHaveParent()
            => Error.Create(
                code: "Region.ProvinceCannotHaveParent",
                message: "استان ریشه‌ی سلسله‌مراتب است و والد ندارد.");

        public static Error ParentRequired(RegionLevel level)
            => Error.Create(
                code: "Region.ParentRequired",
                message: "شهرستان فقط زیر استان و روستا فقط زیر شهرستان تعریف می‌شود؛ منطقه در این سطح بدون والد معتبر نیست.",
                details: new Dictionary<string, object>
                {
                    ["Level"] = level.ToString()
                });

        public static Error InvalidParentLevel(RegionLevel level, RegionLevel? parentLevel)
            => Error.Create(
                code: "Region.InvalidParentLevel",
                message: "سطح والد با سطح این منطقه هم‌خوانی ندارد.",
                details: new Dictionary<string, object>
                {
                    ["Level"] = level.ToString(),
                    ["ParentLevel"] = parentLevel?.ToString() ?? string.Empty
                });

        public static Error SelfParent()
            => Error.Create(
                code: "Region.SelfParent",
                message: "منطقه نمی‌تواند والد خودش باشد.");

        public static Error ParentUnchanged()
            => Error.Create(
                code: "Region.ParentUnchanged",
                message: "والد جدید همان والد فعلی منطقه است.");
    }
}
