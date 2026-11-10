using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Common;
using Fartak.Domain.Framework;
using Fartak.Domain.Plot.Events;
using Fartak.Domain.Region;
using Fartak.Domain.User.Fartak.Domain.User;

namespace Fartak.Domain.Plot
{
    // قطعه زمین — موجودیت مستقل و مالکیت‌دارِ کشاورز (جدول plots)
    // یک پرونده (Case) بعداً روی «یک قطعه × یک محصول × یک فصل» ساخته می‌شود
    public class Plot : AggregateRoot<PlotId>
    {
        public const int MaxTitleLength = 120;

        private const int MaxSoilTextureLength = 60;
        private const int MaxLabNameLength = 120;
        private const int MaxGeoJsonLength = 1_000_000;

        public UserId FarmerId { get; private set; }              // مالک قطعه (کشاورز)
        public string Title { get; private set; }                 // مثل «X1»
        public RegionId? Region { get; private set; }             // موقعیت جغرافیایی (جدول regions)
        public string? Address { get; private set; }
        public string? GeoJson { get; private set; }              // پلی‌گان GeoJSON (jsonb)
        public Area Area { get; private set; }                    // مساحت (هکتار)
        public PlotAreaSource AreaSource { get; private set; }    // منبع مساحت
        public string? SoilTexture { get; private set; }          // نوع خاک: لومی، رسی، شنی و ...
        public LookupId? WaterSourceId { get; private set; }      // منبع آب (lookup: water_source)
        public LookupId? IrrigationTypeId { get; private set; }   // نوع آبیاری (lookup: irrigation_type)
        public WaterQuota? WaterQuota { get; private set; }       // سهمیه/قابلیت آب (مترمکعب)
        public bool IsActive { get; private set; }
        public string? Meta { get; private set; }                 // jsonb آزاد (اعتبارسنجی ساختار در لایه Application)
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        private readonly List<SoilTest> _soilTests = new();
        public IReadOnlyCollection<SoilTest> SoilTests => _soilTests.AsReadOnly();

        private Plot() { } // برای EF Core

        private Plot(PlotId id, UserId farmerId, string title, Area area, PlotAreaSource areaSource)
        {
            Id = id;
            FarmerId = farmerId;
            Title = title;
            Area = area;
            AreaSource = areaSource;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = CreatedAt;
        }

        public static Result<Plot> Create(
            PlotId id,
            UserId farmerId,
            string title,
            Area area,
            PlotAreaSource areaSource,
            RegionId? region = null,
            string? address = null,
            string? geoJson = null,
            string? soilTexture = null,
            LookupId? waterSourceId = null,
            LookupId? irrigationTypeId = null,
            WaterQuota? waterQuota = null,
            string? meta = null)
        {
            var validation = Validate(id, farmerId, title, area, areaSource, geoJson, soilTexture);
            if (validation.IsFailure)
                return Result.Failure<Plot>(validation.Error);

            var plot = new Plot(id, farmerId, title.Trim(), area, areaSource);
            plot.Region = region;
            plot.Address = string.IsNullOrWhiteSpace(address) ? null : address.Trim();
            plot.GeoJson = string.IsNullOrWhiteSpace(geoJson) ? null : geoJson.Trim();
            plot.SoilTexture = string.IsNullOrWhiteSpace(soilTexture) ? null : soilTexture.Trim();
            plot.WaterSourceId = waterSourceId;
            plot.IrrigationTypeId = irrigationTypeId;
            plot.WaterQuota = waterQuota;
            plot.Meta = meta;

            plot.AddDomainEvent(new PlotCreatedEvent(id.Value, farmerId.Value, plot.Title, area.Hectares));
            return Result<Plot>.Success(plot);
        }

        // ویرایش اطلاعات قطعه زمین (فرم ویرایش)
        public Result Update(
            string title,
            Area area,
            PlotAreaSource areaSource,
            RegionId? region = null,
            string? address = null,
            string? geoJson = null,
            string? soilTexture = null,
            LookupId? waterSourceId = null,
            LookupId? irrigationTypeId = null,
            WaterQuota? waterQuota = null)
        {
            if (!IsActive)
                return Result.Failure(PlotErrors.PlotNotActiveError(Id.Value));

            var validation = Validate(Id, FarmerId, title, area, areaSource, geoJson, soilTexture);
            if (validation.IsFailure)
                return Result.Failure(validation.Error);

            Title = title.Trim();
            Area = area;
            AreaSource = areaSource;
            Region = region;
            Address = string.IsNullOrWhiteSpace(address) ? null : address.Trim();
            GeoJson = string.IsNullOrWhiteSpace(geoJson) ? null : geoJson.Trim();
            SoilTexture = string.IsNullOrWhiteSpace(soilTexture) ? null : soilTexture.Trim();
            WaterSourceId = waterSourceId;
            IrrigationTypeId = irrigationTypeId;
            WaterQuota = waterQuota;
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new PlotUpdatedEvent(Id.Value, FarmerId.Value));
            return Result.Success();
        }

        public Result Deactivate()
        {
            if (!IsActive)
                return Result.Failure(PlotErrors.PlotAlreadyDeactivatedError(Id.Value));

            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
            AddDomainEvent(new PlotDeactivatedEvent(Id.Value, FarmerId.Value));
            return Result.Success();
        }

        public Result Activate()
        {
            if (IsActive)
                return Result.Failure(PlotErrors.PlotAlreadyActiveError(Id.Value));

            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
            AddDomainEvent(new PlotActivatedEvent(Id.Value, FarmerId.Value));
            return Result.Success();
        }

        // ثبت نتیجه تست خاک — همیشه از طریق خود Plot (نه مستقیم روی SoilTest)
        public Result<SoilTest> RecordSoilTest(
            SoilTestId soilTestId,
            DateOnly testedAt,
            SoilAnalysis analysis,
            string? labName = null,
            string? summary = null,
            UserId? recordedBy = null)
        {
            if (soilTestId is null || analysis is null)
                return Result.Failure<SoilTest>(PlotErrors.NullReferenceError());

            if (!IsActive)
                return Result.Failure<SoilTest>(PlotErrors.PlotNotActiveError(Id.Value));

            if (testedAt > DateOnly.FromDateTime(DateTime.UtcNow))
                return Result.Failure<SoilTest>(PlotErrors.SoilTestDateInFuture(testedAt));

            if (!string.IsNullOrWhiteSpace(labName) && labName.Trim().Length > MaxLabNameLength)
                return Result.Failure<SoilTest>(PlotErrors.InvalidLabName(labName));

            var soilTest = SoilTest.Create(
                soilTestId,
                Id,
                testedAt,
                analysis,
                string.IsNullOrWhiteSpace(labName) ? null : labName.Trim(),
                string.IsNullOrWhiteSpace(summary) ? null : summary.Trim(),
                recordedBy);

            _soilTests.Add(soilTest);
            AddDomainEvent(new SoilTestRecordedEvent(Id.Value, soilTestId.Value, testedAt));
            return Result<SoilTest>.Success(soilTest);
        }

        // قواعد مشترک Create و Update
        private static Result Validate(
            PlotId id,
            UserId farmerId,
            string title,
            Area area,
            PlotAreaSource areaSource,
            string? geoJson,
            string? soilTexture)
        {
            if (id is null || farmerId is null || area is null)
                return Result.Failure(PlotErrors.NullReferenceError());

            if (string.IsNullOrWhiteSpace(title))
                return Result.Failure(PlotErrors.InvalidTitle(title));

            if (title.Trim().Length > MaxTitleLength)
                return Result.Failure(PlotErrors.TitleTooLong(title.Trim()));

            if (!string.IsNullOrWhiteSpace(geoJson))
            {
                var geo = geoJson.Trim();
                if (!geo.StartsWith('{') || geo.Length > MaxGeoJsonLength)
                    return Result.Failure(PlotErrors.InvalidGeoJson());
            }

            if (!string.IsNullOrWhiteSpace(soilTexture) && soilTexture.Trim().Length > MaxSoilTextureLength)
                return Result.Failure(PlotErrors.InvalidSoilTexture(soilTexture.Trim()));

            // مساحت «محاسبه‌شده» یعنی از روی پلی‌گان محاسبه شده؛ بدون پلی‌گان بی‌معناست
            if (areaSource is PlotAreaSource.Computed && string.IsNullOrWhiteSpace(geoJson))
                return Result.Failure(PlotErrors.ComputedAreaRequiresGeoJson());

            return Result.Success();
        }
    }
}
