using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.ExpertProfile.Events;
using Fartak.Domain.Framework;
using Fartak.Domain.Region;
using Fartak.Domain.User.Fartak.Domain.User;

namespace Fartak.Domain.ExpertProfile
{
    // پروفایل کارشناس (جدول expert_profiles) + مناطق تحت پوشش (جدول expert_regions)
    //
    // تصمیم شناسه: PK این جدول user_id است (رابطه ۱:۱ با کاربر) — پس شناسه تجمع همان UserId است.
    //
    // قواعد ظرفیت (مطابق ERD با پیش‌فرض ۱۲ و ۲):
    //   MaxActiveCases بین ۰ تا ۱۰۰ — مقدار ۰ یعنی تا اطلاع ثانوی پرونده جدید نگیرد
    //   MaxDailyVisits بین ۰ تا ۲۰
    //
    // تصمیم غیرفعال‌سازی: مناطق تحت پوشش حفظ می‌شوند (پس از فعال‌سازی دوباره از بین نمی‌روند)؛
    // جلوگیری از تخصیص پرونده/بازدید جدید به کارشناسِ غیرفعال در لایه Application با چک IsActive انجام می‌شود.
    // تغییر مناطق فقط در حالت فعال مجاز است.
    public class ExpertProfile : AggregateRoot<UserId>
    {
        public const int MaxSpecialtyLength = 120;
        public const int MaxActiveCasesLimit = 100;
        public const int MaxDailyVisitsLimit = 20;
        public const int DefaultMaxActiveCases = 12;
        public const int DefaultMaxDailyVisits = 2;

        public string? Specialty { get; private set; }         // مثل «کارشناس فنی کشت گندم»
        public int MaxActiveCases { get; private set; }
        public int MaxDailyVisits { get; private set; }
        public bool IsActive { get; private set; }
        public string? Meta { get; private set; }              // jsonb آزاد (اعتبارسنجی ساختار در لایه Application)

        private readonly List<ExpertRegion> _coveredRegions = new();
        public IReadOnlyCollection<ExpertRegion> CoveredRegions => _coveredRegions.AsReadOnly();

        private ExpertProfile() { } // برای EF Core

        private ExpertProfile(UserId userId, int maxActiveCases, int maxDailyVisits)
        {
            Id = userId;
            MaxActiveCases = maxActiveCases;
            MaxDailyVisits = maxDailyVisits;
            IsActive = true;
        }

        public static Result<ExpertProfile> Create(
            UserId userId,
            string? specialty = null,
            int maxActiveCases = DefaultMaxActiveCases,
            int maxDailyVisits = DefaultMaxDailyVisits,
            string? meta = null)
        {
            if (userId is null)
                return Result.Failure<ExpertProfile>(ExpertProfileErrors.NullReferenceError());

            var validation = Validate(specialty, maxActiveCases, maxDailyVisits);
            if (validation.IsFailure)
                return Result.Failure<ExpertProfile>(validation.Error);

            var profile = new ExpertProfile(userId, maxActiveCases, maxDailyVisits);
            profile.Specialty = Normalize(specialty);
            profile.Meta = meta;

            profile.AddDomainEvent(new ExpertProfileCreatedEvent(
                userId.Value, profile.MaxActiveCases, profile.MaxDailyVisits));
            return Result<ExpertProfile>.Success(profile);
        }

        // ویرایش جزئی: هر پارامتر تهی یعنی «بدون تغییر» (تفاوت عمدی با FarmerProfile که فرم کامل است)
        // برای پاک‌کردن تخصص، رشته تهی («») بفرست؛ برای پاک‌کردن Meta همین‌طور
        public Result Update(
            string? specialty = null,
            int? maxActiveCases = null,
            int? maxDailyVisits = null,
            string? meta = null)
        {
            if (specialty is not null || maxActiveCases is not null || maxDailyVisits is not null)
            {
                var validation = Validate(
                    specialty ?? Specialty,
                    maxActiveCases ?? MaxActiveCases,
                    maxDailyVisits ?? MaxDailyVisits);
                if (validation.IsFailure)
                    return validation;
            }

            if (specialty is not null)
                Specialty = Normalize(specialty);

            if (maxActiveCases is not null)
                MaxActiveCases = maxActiveCases.Value;

            if (maxDailyVisits is not null)
                MaxDailyVisits = maxDailyVisits.Value;

            if (meta is not null)
                Meta = Normalize(meta);

            AddDomainEvent(new ExpertProfileUpdatedEvent(Id.Value));
            return Result.Success();
        }

        public Result Activate()
        {
            if (IsActive)
                return Result.Failure(ExpertProfileErrors.ExpertAlreadyActive(Id.Value));

            IsActive = true;
            AddDomainEvent(new ExpertProfileActivatedEvent(Id.Value));
            return Result.Success();
        }

        // مناطق تحت پوشش حذف نمی‌شوند — فقط تخصیص‌های جدید بسته می‌شود (در Application)
        public Result Deactivate()
        {
            if (!IsActive)
                return Result.Failure(ExpertProfileErrors.ExpertAlreadyDeactivated(Id.Value));

            IsActive = false;
            AddDomainEvent(new ExpertProfileDeactivatedEvent(Id.Value));
            return Result.Success();
        }

        // افزودن یک منطقه به مناطق تحت پوشش (کلید/دکمه در UI)
        public Result AddCoveredRegion(RegionId regionId)
        {
            if (regionId is null)
                return Result.Failure(ExpertProfileErrors.NullReferenceError());

            if (!IsActive)
                return Result.Failure(ExpertProfileErrors.ExpertNotActive(Id.Value));

            if (_coveredRegions.Any(r => r.Id == regionId))
                return Result.Failure(ExpertProfileErrors.RegionAlreadyCovered(Id.Value, regionId.Value));

            _coveredRegions.Add(ExpertRegion.Create(regionId));
            AddDomainEvent(new ExpertRegionAssignedEvent(Id.Value, regionId.Value));
            return Result.Success();
        }

        // حذف یک منطقه از مناطق تحت پوشش
        public Result RemoveCoveredRegion(RegionId regionId)
        {
            if (regionId is null)
                return Result.Failure(ExpertProfileErrors.NullReferenceError());

            if (!IsActive)
                return Result.Failure(ExpertProfileErrors.ExpertNotActive(Id.Value));

            var item = _coveredRegions.FirstOrDefault(r => r.Id == regionId);
            if (item is null)
                return Result.Failure(ExpertProfileErrors.RegionNotCovered(Id.Value, regionId.Value));

            _coveredRegions.Remove(item);
            AddDomainEvent(new ExpertRegionRemovedEvent(Id.Value, regionId.Value));
            return Result.Success();
        }

        // جایگزینی کامل مجموعه مناطق (فرم ویرایش با چندانتخابی)
        // رویداد به‌ازای هر منطقه افزوده/حذف‌شده صادر می‌شود؛ ورودی تکراری خودکار یکتا می‌شود
        public Result SetCoveredRegions(IReadOnlyCollection<RegionId> regionIds)
        {
            if (regionIds is null)
                return Result.Failure(ExpertProfileErrors.NullReferenceError());

            if (!IsActive)
                return Result.Failure(ExpertProfileErrors.ExpertNotActive(Id.Value));

            if (regionIds.Any(r => r is null))
                return Result.Failure(ExpertProfileErrors.NullReferenceError());

            var targetSet = regionIds.Distinct().ToList();
            var currentSet = _coveredRegions.Select(r => r.Id).ToList();

            foreach (var current in currentSet)
            {
                if (targetSet.Contains(current))
                    continue;

                RemoveItem(current);
                AddDomainEvent(new ExpertRegionRemovedEvent(Id.Value, current.Value));
            }

            foreach (var target in targetSet)
            {
                if (currentSet.Contains(target))
                    continue;

                _coveredRegions.Add(ExpertRegion.Create(target));
                AddDomainEvent(new ExpertRegionAssignedEvent(Id.Value, target.Value));
            }

            return Result.Success();
        }

        private void RemoveItem(RegionId regionId)
        {
            var item = _coveredRegions.FirstOrDefault(r => r.Id == regionId);
            if (item is not null)
                _coveredRegions.Remove(item);
        }

        private static string? Normalize(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        // قواعد مشترک Create و Update
        private static Result Validate(string? specialty, int maxActiveCases, int maxDailyVisits)
        {
            if (!string.IsNullOrWhiteSpace(specialty) && specialty.Trim().Length > MaxSpecialtyLength)
                return Result.Failure(ExpertProfileErrors.InvalidSpecialty(specialty.Trim()));

            if (maxActiveCases < 0 || maxActiveCases > MaxActiveCasesLimit)
                return Result.Failure(ExpertProfileErrors.InvalidMaxActiveCases(maxActiveCases));

            if (maxDailyVisits < 0 || maxDailyVisits > MaxDailyVisitsLimit)
                return Result.Failure(ExpertProfileErrors.InvalidMaxDailyVisits(maxDailyVisits));

            return Result.Success();
        }
    }
}
