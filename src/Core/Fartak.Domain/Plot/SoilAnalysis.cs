using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Plot
{
    // نتیجه ساختاریافته تست خاک — معادل ستون params (jsonb) در جدول soil_tests
    // ساختار ذخیره‌سازی: {"ph":7.8,"ec_ds_m":2.1,"n_ppm":..,"p_ppm":..,"k_ppm":..,"oc_pct":..}
    public sealed class SoilAnalysis : ValueObject
    {
        public const decimal MinPh = 0m;
        public const decimal MaxPh = 14m;
        public const decimal MaxEcDsM = 50m;          // سقف مجاز هدایت الکتریکی (dS/m)
        public const decimal MaxNutrientPpm = 1000m;  // سقف مجاز برای نیتروژن و فسفر
        public const decimal MaxPotassiumPpm = 2000m; // سقف مجاز برای پتاسیم
        public const decimal MaxOrganicCarbonPct = 100m;

        public decimal Ph { get; }                // اسیدیته خاک (۰ تا ۱۴)
        public decimal EcDsM { get; }             // هدایت الکتریکی (دسی‌زیمنس بر متر)
        public decimal NitrogenPpm { get; }       // نیتروژن (ppm)
        public decimal PhosphorusPpm { get; }     // فسفر (ppm)
        public decimal PotassiumPpm { get; }      // پتاسیم (ppm)
        public decimal OrganicCarbonPct { get; }  // کربن آلی (درصد)

        private SoilAnalysis() { } // برای EF Core

        private SoilAnalysis(
            decimal ph,
            decimal ecDsM,
            decimal nitrogenPpm,
            decimal phosphorusPpm,
            decimal potassiumPpm,
            decimal organicCarbonPct)
        {
            Ph = ph;
            EcDsM = ecDsM;
            NitrogenPpm = nitrogenPpm;
            PhosphorusPpm = phosphorusPpm;
            PotassiumPpm = potassiumPpm;
            OrganicCarbonPct = organicCarbonPct;
        }

        public static SoilAnalysis Create(
            decimal ph,
            decimal ecDsM,
            decimal nitrogenPpm,
            decimal phosphorusPpm,
            decimal potassiumPpm,
            decimal organicCarbonPct)
        {
            var roundedPh = Math.Round(ph, 2);
            if (roundedPh < MinPh || roundedPh > MaxPh)
                throw new DomainValidationException(
                    "Plot.InvalidSoilPh",
                    "اسیدیته خاک (pH) باید عددی بین ۰ تا ۱۴ باشد.",
                    new Dictionary<string, object> { ["Ph"] = ph });

            var roundedEc = Math.Round(ecDsM, 2);
            if (roundedEc < 0 || roundedEc > MaxEcDsM)
                throw new DomainValidationException(
                    "Plot.InvalidSoilEc",
                    "هدایت الکتریکی خاک (EC) نامعتبر است.",
                    new Dictionary<string, object> { ["EcDsM"] = ecDsM });

            var roundedN = Math.Round(nitrogenPpm, 2);
            if (roundedN < 0 || roundedN > MaxNutrientPpm)
                throw new DomainValidationException(
                    "Plot.InvalidSoilNitrogen",
                    "مقدار نیتروژن خاک (ppm) نامعتبر است.",
                    new Dictionary<string, object> { ["NitrogenPpm"] = nitrogenPpm });

            var roundedP = Math.Round(phosphorusPpm, 2);
            if (roundedP < 0 || roundedP > MaxNutrientPpm)
                throw new DomainValidationException(
                    "Plot.InvalidSoilPhosphorus",
                    "مقدار فسفر خاک (ppm) نامعتبر است.",
                    new Dictionary<string, object> { ["PhosphorusPpm"] = phosphorusPpm });

            var roundedK = Math.Round(potassiumPpm, 2);
            if (roundedK < 0 || roundedK > MaxPotassiumPpm)
                throw new DomainValidationException(
                    "Plot.InvalidSoilPotassium",
                    "مقدار پتاسیم خاک (ppm) نامعتبر است.",
                    new Dictionary<string, object> { ["PotassiumPpm"] = potassiumPpm });

            var roundedOc = Math.Round(organicCarbonPct, 2);
            if (roundedOc < 0 || roundedOc > MaxOrganicCarbonPct)
                throw new DomainValidationException(
                    "Plot.InvalidSoilOrganicCarbon",
                    "درصد کربن آلی خاک نامعتبر است.",
                    new Dictionary<string, object> { ["OrganicCarbonPct"] = organicCarbonPct });

            return new SoilAnalysis(roundedPh, roundedEc, roundedN, roundedP, roundedK, roundedOc);
        }

        // خروجی با همان کلیدهای jsonb جدول soil_tests — برای نگاشت Infrastructure
        public IReadOnlyDictionary<string, decimal> ToDictionary()
            => new Dictionary<string, decimal>
            {
                ["ph"] = Ph,
                ["ec_ds_m"] = EcDsM,
                ["n_ppm"] = NitrogenPpm,
                ["p_ppm"] = PhosphorusPpm,
                ["k_ppm"] = PotassiumPpm,
                ["oc_pct"] = OrganicCarbonPct
            };

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Ph;
            yield return EcDsM;
            yield return NitrogenPpm;
            yield return PhosphorusPpm;
            yield return PotassiumPpm;
            yield return OrganicCarbonPct;
        }

        public override string ToString()
            => $"pH={Ph} | EC={EcDsM}dS/m | N={NitrogenPpm} | P={PhosphorusPpm} | K={PotassiumPpm} | OC={OrganicCarbonPct}%";
    }
}

