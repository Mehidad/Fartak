using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Common;
using Fartak.Domain.Framework;
using Fartak.Domain.User.Events;
using Fartak.Domain.User.Fartak.Domain.User;
using static System.Net.WebRequestMethods;

namespace Fartak.Domain.User
{
    public class User : AggregateRoot<UserId>
    {
        // اطلاعات اصلی
        public Phone PhoneNumber { get; private set; }
        public long? RoleId { get; private set; } // نقش کاربر — FK به Aggregate Role (null = هنوز انتخاب نشده)
        public string? FullName { get; private set; } // نام و نام خانوادگی — برای درخواست‌های فرتاک لازم است
        public OTP? OtpCode { get; private set; } // کد تایید ارسال شده برای ورود
        public RefreshToken? RefreshToken { get; private set; }

        // وضعیت
        public DateTime CreatedAt { get; private set; }

        public bool HasRole => RoleId.HasValue;

        private User() { } // برای EF Core

        private User(UserId id, Phone phone)
        {
            Id = id;
            PhoneNumber = phone;
            CreatedAt = DateTime.UtcNow;
        }

        public static Result<User> Create(UserId id, Phone phoneNumber)
        {
            var user = new User(id, phoneNumber);
            user.AddDomainEvent(new UserCreatedEvent(id.Value, phoneNumber.Value));
            return Result<User>.Success(user);
        }

        // متدهای Domain
        public Result AssignRole(long roleId)
        {
            if (roleId <= 0)
                return Result.Failure(UserErrors.InvalidRoleIdError(Id.Value));

            if (RoleId == roleId)
                return Result.Failure(UserErrors.RoleAlreadyAssignedError(Id.Value, roleId));

            RoleId = roleId;
            AddDomainEvent(new UserRoleAssignedEvent(Id.Value, roleId));
            return Result.Success();
        }

        public Result SetProfile(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return Result.Failure(UserErrors.EmptyFullNameError(Id.Value));

            FullName = fullName.Trim();
            return Result.Success();
        }

        public Result SetOtp(OTP otp)
        {
            OtpCode = otp;
            AddDomainEvent(new OtpSetupedEvent(Id.Value, OtpCode.Code));
            return Result.Success();
        }

        public Result VerifyOtp(string code)
        {
            if (OtpCode == null)
                return Result.Failure(UserErrors.NullOtpError(Id.Value));

            if (!OtpCode.IsValid(code))
                return Result.Failure(UserErrors.InvalidOtpError(Id.Value));

            AddDomainEvent(new OtpVerifiedEvent(Id.Value));
            OtpCode = null;
            return Result.Success();
        }

        public void SetRefreshToken(RefreshToken token) => RefreshToken = token;
        public void ClearRefreshToken() => RefreshToken = null;
    }
}
