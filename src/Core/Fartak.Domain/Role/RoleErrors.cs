using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Role
{
    public static class RoleErrors
    {
        public static Error NotFound(long roleId)
            => Error.Create(
                code: "Role.NotFound",
                message: "نقش مورد نظر یافت نشد",
                details: new Dictionary<string, object>
                {
                    ["RoleId"] = roleId
                });

        public static Error EmptyTitle()
            => Error.Create(
                code: "Role.EmptyTitle",
                message: "عنوان نقش الزامی است",
                details: new Dictionary<string, object>
                {
                    ["Field"] = "Title"
                });

        public static Error EmptyCode()
            => Error.Create(
                code: "Role.EmptyCode",
                message: "کد نقش الزامی است",
                details: new Dictionary<string, object>
                {
                    ["Field"] = "Code"
                });

        public static Error InvalidCodeFormat(string code)
            => Error.Create(
                code: "Role.InvalidCodeFormat",
                message: "کد نقش فقط می‌تواند شامل حروف لاتین، عدد و _ باشد",
                details: new Dictionary<string, object>
                {
                    ["Code"] = code
                });

        public static Error RoleDeleted(long roleId)
            => Error.Create(
                code: "Role.Deleted",
                message: "نقش حذف‌شده قابل تغییر نیست",
                details: new Dictionary<string, object>
                {
                    ["RoleId"] = roleId
                });

        public static Error RoleAlreadyActive(long roleId)
            => Error.Create(
                code: "Role.AlreadyActive",
                message: "نقش در حال حاضر فعال است",
                details: new Dictionary<string, object>
                {
                    ["RoleId"] = roleId
                });

        public static Error RoleAlreadyDeactivated(long roleId)
            => Error.Create(
                code: "Role.AlreadyDeactivated",
                message: "نقش در حال حاضر غیرفعال است",
                details: new Dictionary<string, object>
                {
                    ["RoleId"] = roleId
                });

        public static Error SystemRoleCannotBeDeleted(long roleId)
            => Error.Create(
                code: "Role.SystemCannotBeDeleted",
                message: "نقش سیستمی قابل حذف نیست",
                details: new Dictionary<string, object>
                {
                    ["RoleId"] = roleId
                });

        public static Error SystemRoleCannotBeDeactivated(long roleId)
            => Error.Create(
                code: "Role.SystemCannotBeDeactivated",
                message: "نقش سیستمی قابل غیرفعال شدن نیست",
                details: new Dictionary<string, object>
                {
                    ["RoleId"] = roleId
                });
        public static Error CodeAlreadyExists(string code) =>
    Error.Create("Role.Code.Duplicate", $"نقشی با کد '{code}' از قبل وجود دارد.");

      }
}
