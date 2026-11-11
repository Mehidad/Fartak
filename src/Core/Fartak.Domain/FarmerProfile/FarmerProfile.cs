using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.FarmerProfile.Events;
using Fartak.Domain.Framework;
using Fartak.Domain.Region;
using Fartak.Domain.User.Fartak.Domain.User;

namespace Fartak.Domain.FarmerProfile
{
    // اطلاعات هویتی، آدرس و حساب بانکی کشاورز (جدول farmer_profiles)
    //
    // تصمیم شناسه: PK این جدول user_id است (رابطه ۱:۱ با کاربر) — پس این تجمع شناسه جدا
    // (FarmerProfileId) نمی‌گیرد و Id آن همان UserId است؛ ساخت پروفایل در Application
    // پس از اطمینان از وجود کاربر انجام می‌شود.
    //
    // همه فیلدهای محتوایی مطابق ERD اختیاری‌اند؛ اعتبارسنجی سخت‌گیرانه فقط روی فیلدهای
    // پرشده انجام می‌شود (VOها هنگام ساخت معتبر می‌شوند و اینجا فقط طول/رقم بودن فیلدهای ساده چک می‌شود).
    public class FarmerProfile : AggregateRoot<UserId>
    {
        public const int MaxVillageLength = 120;
        public const int MaxBankNameLength = 100;
        public const int MaxAccountNoLength = 30;

        private const int MinFixedPhoneLength = 8;
        private const int MaxFixedPhoneLength = 15;

        public NationalId? NationalId { get; private set; }     // کد ملی (در صورت ارسال: رقم کنترل معتبر)
        public PostalCode? PostalCode { get; private set; }     // کد پستی (۱۰ رقم)
        public string? FixedPhone { get; private set; }         // تلفن ثابت (فقط رقم)
        public RegionId? Region { get; private set; }           // موقعیت جغرافیایی (جدول regions)
        public string? Village { get; private set; }
        public string? Address { get; private set; }            // متن آزاد
        public string? BankName { get; private set; }
        public Sheba? Sheba { get; private set; }               // شبا (IR + ۲۴ رقم؛ mod-97)
        public string? AccountNo { get; private set; }          // شماره حساب (فقط رقم)
        public string? Meta { get; private set; }               // jsonb آزاد (اعتبارسنجی ساختار در لایه Application)
        public DateTime UpdatedAt { get; private set; }         // مطابق ERD بدون CreatedAt

        private FarmerProfile() { } // برای EF Core

        private FarmerProfile(UserId userId)
        {
            Id = userId;
        }

        public static Result<FarmerProfile> Create(
            UserId userId,
            NationalId? nationalId = null,
            PostalCode? postalCode = null,
            string? fixedPhone = null,
            RegionId? regionId = null,
            string? village = null,
            string? address = null,
            string? bankName = null,
            Sheba? sheba = null,
            string? accountNo = null,
            string? meta = null)
        {
            if (userId is null)
                return Result.Failure<FarmerProfile>(FarmerProfileErrors.NullReferenceError());

            var validation = ValidateStrings(fixedPhone, village, bankName, accountNo);
            if (validation.IsFailure)
                return Result.Failure<FarmerProfile>(validation.Error);

            var profile = new FarmerProfile(userId);
            profile.NationalId = nationalId;
            profile.PostalCode = postalCode;
            profile.FixedPhone = Normalize(fixedPhone);
            profile.Region = regionId;
            profile.Village = Normalize(village);
            profile.Address = Normalize(address);
            profile.BankName = Normalize(bankName);
            profile.Sheba = sheba;
            profile.AccountNo = Normalize(accountNo);
            profile.Meta = meta;
            profile.UpdatedAt = DateTime.UtcNow;

            profile.AddDomainEvent(new FarmerProfileCreatedEvent(userId.Value));
            return Result<FarmerProfile>.Success(profile);
        }

        // ویرایش کامل پروفایل (فرم ویرایش) — مقدار تهی یعنی پاک‌شدن آن فیلد
        public Result Update(
            NationalId? nationalId = null,
            PostalCode? postalCode = null,
            string? fixedPhone = null,
            RegionId? regionId = null,
            string? village = null,
            string? address = null,
            string? bankName = null,
            Sheba? sheba = null,
            string? accountNo = null,
            string? meta = null)
        {
            var validation = ValidateStrings(fixedPhone, village, bankName, accountNo);
            if (validation.IsFailure)
                return validation;

            NationalId = nationalId;
            PostalCode = postalCode;
            FixedPhone = Normalize(fixedPhone);
            Region = regionId;
            Village = Normalize(village);
            Address = Normalize(address);
            BankName = Normalize(bankName);
            Sheba = sheba;
            AccountNo = Normalize(accountNo);
            Meta = meta;
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new FarmerProfileUpdatedEvent(Id.Value));
            return Result.Success();
        }

        private static string? Normalize(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        // قواعد مشترک Create و Update روی فیلدهای ساده (VOها هنگام ساخت خودشان اعتبار می‌گیرند)
        private static Result ValidateStrings(
            string? fixedPhone,
            string? village,
            string? bankName,
            string? accountNo)
        {
            if (!string.IsNullOrWhiteSpace(fixedPhone))
            {
                var phone = fixedPhone.Trim();
                if (phone.Length < MinFixedPhoneLength
                    || phone.Length > MaxFixedPhoneLength
                    || !phone.All(char.IsDigit))
                    return Result.Failure(FarmerProfileErrors.InvalidFixedPhone(phone));
            }

            if (!string.IsNullOrWhiteSpace(village) && village.Trim().Length > MaxVillageLength)
                return Result.Failure(FarmerProfileErrors.VillageTooLong(village.Trim()));

            if (!string.IsNullOrWhiteSpace(bankName) && bankName.Trim().Length > MaxBankNameLength)
                return Result.Failure(FarmerProfileErrors.BankNameTooLong(bankName.Trim()));

            if (!string.IsNullOrWhiteSpace(accountNo))
            {
                var account = accountNo.Trim();
                if (account.Length > MaxAccountNoLength || !account.All(char.IsDigit))
                    return Result.Failure(FarmerProfileErrors.InvalidAccountNo(account));
            }

            return Result.Success();
        }
    }
}
