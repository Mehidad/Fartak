using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.ExpertProfile
{
    public static class ExpertProfileErrors
    {
        public static Error NullReferenceError()
            => Error.Create(
                code: "ExpertProfile.NullReference",
                message: "مقادیر اجباری پروفایل کارشناس ارسال نشده است.");

        public static Error InvalidSpecialty(string specialty)
            => Error.Create(
                code: "ExpertProfile.InvalidSpecialty",
                message: "تخصص کارشناس نباید بیش از ۱۲۰ کاراکتر باشد.",
                details: new Dictionary<string, object>
                {
                    ["Specialty"] = specialty
                });

        public static Error InvalidMaxActiveCases(int maxActiveCases)
            => Error.Create(
                code: "ExpertProfile.InvalidMaxActiveCases",
                message: "حداکثر پرونده‌های فعال باید عددی بین ۰ تا ۱۰۰ باشد.",
                details: new Dictionary<string, object>
                {
                    ["MaxActiveCases"] = maxActiveCases
                });

        public static Error InvalidMaxDailyVisits(int maxDailyVisits)
            => Error.Create(
                code: "ExpertProfile.InvalidMaxDailyVisits",
                message: "حداکثر بازدیدهای روزانه باید عددی بین ۰ تا ۲۰ باشد.",
                details: new Dictionary<string, object>
                {
                    ["MaxDailyVisits"] = maxDailyVisits
                });

        public static Error ExpertAlreadyActive(long userId)
            => Error.Create(
                code: "ExpertProfile.AlreadyActive",
                message: "پروفایل این کارشناس از قبل فعال است.",
                details: new Dictionary<string, object>
                {
                    ["UserId"] = userId
                });

        public static Error ExpertAlreadyDeactivated(long userId)
            => Error.Create(
                code: "ExpertProfile.AlreadyDeactivated",
                message: "پروفایل این کارشناس از قبل غیرفعال است.",
                details: new Dictionary<string, object>
                {
                    ["UserId"] = userId
                });

        public static Error ExpertNotActive(long userId)
            => Error.Create(
                code: "ExpertProfile.NotActive",
                message: "پروفایل این کارشناس غیرفعال است؛ تغییر مناطق تحت پوشش مجاز نیست.",
                details: new Dictionary<string, object>
                {
                    ["UserId"] = userId
                });

        public static Error RegionAlreadyCovered(long userId, long regionId)
            => Error.Create(
                code: "ExpertProfile.RegionAlreadyCovered",
                message: "این منطقه از قبل در مناطق تحت پوشش کارشناس وجود دارد.",
                details: new Dictionary<string, object>
                {
                    ["UserId"] = userId,
                    ["RegionId"] = regionId
                });

        public static Error RegionNotCovered(long userId, long regionId)
            => Error.Create(
                code: "ExpertProfile.RegionNotCovered",
                message: "این منطقه در مناطق تحت پوشش کارشناس وجود ندارد.",
                details: new Dictionary<string, object>
                {
                    ["UserId"] = userId,
                    ["RegionId"] = regionId
                });
    }
}
