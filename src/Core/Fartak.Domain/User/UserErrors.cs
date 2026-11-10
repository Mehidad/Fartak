using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.User
{
    public static class UserErrors
    {
        public static Error NullOtpError(long userId)
           => Error.Create(
               code: "User.NullOtp",
               message: "کد OTP وجود ندارد.",
               details: new Dictionary<string, object>
               {
                   ["UserId"] = userId
               });

        public static Error InvalidOtpError(long userId)
            => Error.Create(
                code: "User.InvalidOtp",
                message: "کد OTP نامعتبر یا منقضی شده است.",
                details: new Dictionary<string, object>
                {
                    ["UserId"] = userId
                });

        public static Error EmptyFullNameError(long userId)
            => Error.Create(
                code: "User.EmptyFullName",
                message: "نام و نام خانوادگی نمی‌تواند خالی باشد.",
                details: new Dictionary<string, object>
                {
                    ["UserId"] = userId
                });

        public static Error InvalidRoleIdError(long userId)
            => Error.Create(
                code: "User.InvalidRoleId",
                message: "شناسه نقش ارسالی نامعتبر است.",
                details: new Dictionary<string, object>
                {
                    ["UserId"] = userId
                });

        public static Error RoleAlreadyAssignedError(long userId, long roleId)
            => Error.Create(
                code: "User.RoleAlreadyAssigned",
                message: "همین نقش قبلاً به کاربر تخصیص یافته است.",
                details: new Dictionary<string, object>
                {
                    ["UserId"] = userId,
                    ["RoleId"] = roleId
                });
    }
}
