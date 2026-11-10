using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Plot
{
    // مساحت قطعه زمین بر حسب هکتار — معادل ستون area_hectares numeric(10,4)
    public sealed class Area : ValueObject
    {
        public const decimal MinHectares = 0.0001m;
        public const decimal MaxHectares = 1_000_000m;

        public decimal Hectares { get; }

        private Area() { } // برای EF Core

        private Area(decimal hectares)
        {
            Hectares = hectares;
        }

        public static Area Create(decimal hectares)
        {
            var rounded = Math.Round(hectares, 4);

            if (rounded < MinHectares || rounded > MaxHectares)
                throw new DomainValidationException(
                    "Plot.InvalidArea",
                    "مساحت قطعه زمین باید بین ۰٫۰۰۰۱ و ۱٬۰۰۰٬۰۰۰ هکتار باشد.",
                    new Dictionary<string, object> { ["Hectares"] = hectares });

            return new Area(rounded);
        }

        // تبدیل به مترمربع — برای محاسبات و مقایسه با سهمیه آب
        public decimal ToSquareMeters() => Hectares * 10_000m;

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Hectares;
        }

        public override string ToString() => $"{Hectares} ha";
    }
}
