using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;
using Fartak.Domain.Region.Events;

namespace Fartak.Domain.Region
{
    // منطقه در سلسله‌مراتب جغرافیایی سه‌سطحی (جدول regions): استان > شهرستان > روستا
    // جدول فقط چهار ستون دارد؛ بنابراین این تجمع عمداً سبک است: بدون timestamp و بدون IsActive.
    // والد همیشه با شناسه ارجاع می‌شود (نه مالکیت) تا کل درخت در حافظه بارگذاری نشود.
    public class Region : AggregateRoot<RegionId>
    {
        public const int MaxNameLength = 120;

        public RegionId? ParentId { get; private set; }  // والد؛ برای استان همیشه تهی
        public RegionLevel Level { get; private set; }   // سطح در درخت — در ساخت تعیین می‌شود و تغییر نمی‌کند
        public string Name { get; private set; }         // نام نمایشی (مثلاً «فارس»)

        private Region() { } // برای EF Core

        private Region(RegionId id, RegionLevel level, string name, RegionId? parentId)
        {
            Id = id;
            Level = level;
            Name = name;
            ParentId = parentId;
        }

        public static Result<Region> Create(
            RegionId id,
            RegionLevel level,
            string name,
            RegionId? parentId = null,
            RegionLevel? parentLevel = null)
        {
            var hierarchy = ValidateHierarchy(id, level, parentId, parentLevel);
            if (hierarchy.IsFailure)
                return Result.Failure<Region>(hierarchy.Error);

            var nameValidation = ValidateName(name);
            if (nameValidation.IsFailure)
                return Result.Failure<Region>(nameValidation.Error);

            var region = new Region(id, level, name.Trim(), parentId);
            region.AddDomainEvent(new RegionCreatedEvent(id.Value, level, region.Name, parentId?.Value));
            return Result<Region>.Success(region);
        }

        // تغییر نام نمایشی (فرم ویرایش) — سطح و جایگاه در درخت تغییر نمی‌کند
        public Result Rename(string newName)
        {
            var validation = ValidateName(newName);
            if (validation.IsFailure)
                return validation;

            var oldName = Name;
            Name = newName.Trim();
            AddDomainEvent(new RegionRenamedEvent(Id.Value, oldName, Name));
            return Result.Success();
        }

        // جابه‌جایی در درخت: انتقال شهرستان/روستا به والد دیگر (با همان قاعده سطح)
        // سطح والد را لایه Application از روی Aggregate والدِ بارگذاری‌شده پاس می‌دهد
        public Result ChangeParent(RegionId? parentId, RegionLevel? parentLevel)
        {
            var hierarchy = ValidateHierarchy(Id, Level, parentId, parentLevel);
            if (hierarchy.IsFailure)
                return hierarchy;

            if (ParentId?.Value == parentId?.Value)
                return Result.Failure(RegionErrors.ParentUnchanged());

            var oldParentId = ParentId;
            ParentId = parentId;
            AddDomainEvent(new RegionParentChangedEvent(Id.Value, oldParentId?.Value, parentId?.Value));
            return Result.Success();
        }

        // قاعده مشترک Create و ChangeParent — سلسله‌مراتب سه‌سطحی:
        // استان والد ندارد؛ شهرستان فقط زیر استان و روستا فقط زیر شهرستان.
        private static Result ValidateHierarchy(
            RegionId id,
            RegionLevel level,
            RegionId? parentId,
            RegionLevel? parentLevel)
        {
            if (id is null)
                return Result.Failure(RegionErrors.NullReferenceError());

            if (level == RegionLevel.Province)
            {
                if (parentId is not null)
                    return Result.Failure(RegionErrors.ProvinceCannotHaveParent());

                return Result.Success();
            }

            if (parentId is null)
                return Result.Failure(RegionErrors.ParentRequired(level));

            if (parentId.Value == id.Value)
                return Result.Failure(RegionErrors.SelfParent());

            var expectedParentLevel = level == RegionLevel.City
                ? RegionLevel.Province
                : RegionLevel.City;

            if (parentLevel != expectedParentLevel)
                return Result.Failure(RegionErrors.InvalidParentLevel(level, parentLevel));

            return Result.Success();
        }

        private static Result ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result.Failure(RegionErrors.InvalidName(name));

            if (name.Trim().Length > MaxNameLength)
                return Result.Failure(RegionErrors.NameTooLong(name.Trim()));

            return Result.Success();
        }
    }
}
