using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;
using Fartak.Domain.Region;

namespace Fartak.Domain.ExpertProfile
{
    // منطقه تحت پوشش کارشناس (جدول expert_regions — بدون PK مستقل در ERD)
    // موجودیت فرزندِ ExpertProfile؛ هویت آن در این تجمع همان RegionId است
    // (در دیتابیس کلید مرکبی از expert_user_id + region_id ساخته می‌شود)
    // فقط از طریق خود Aggregate ساخته می‌شود: ExpertProfile.AddCoveredRegion / SetCoveredRegions
    public sealed class ExpertRegion : Entity<RegionId>
    {
        private ExpertRegion() { } // برای EF Core

        private ExpertRegion(RegionId regionId)
        {
            Id = regionId;
        }

        internal static ExpertRegion Create(RegionId regionId)
        {
            if (regionId is null)
                throw new DomainValidationException(
                    "ExpertProfile.RegionIdNull",
                    "شناسه منطقه تحت پوشش ارسال نشده است.");

            return new ExpertRegion(regionId);
        }
    }
}
