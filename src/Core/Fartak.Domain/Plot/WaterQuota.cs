using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Plot
{
    // سهمیه/قابلیت آب قطعه بر حسب مترمکعب — معادل ستون water_quota_m3 numeric(12,2)
    public sealed class WaterQuota : ValueObject
    {
        public const decimal MinCubicMeters = 0.01m;
        public const decimal MaxCubicMeters = 1_000_000_000m;

        public decimal CubicMeters { get; }

        private WaterQuota() { } // برای EF Core

        private WaterQuota(decimal cubicMeters)
        {
            CubicMeters = cubicMeters;
        }

        public static WaterQuota Create(decimal cubicMeters)
        {
            var rounded = Math.Round(cubicMeters, 2);

            if (rounded < MinCubicMeters || rounded > MaxCubicMeters)
                throw new DomainValidationException(
                    "Plot.InvalidWaterQuota",
                    "سهمیه آب باید عددی بزرگ‌تر از صفر باشد.",
                    new Dictionary<string, object> { ["CubicMeters"] = cubicMeters });

            return new WaterQuota(rounded);
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return CubicMeters;
        }

        public override string ToString() => $"{CubicMeters} m3";
    }
}
