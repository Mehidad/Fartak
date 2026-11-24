using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Plot
{
    // شناسه یک رکورد مشاهده هواشناسی (جدول weather_observations)
    public sealed record WeatherObservationId
    {
        public long Value { get; }

        private WeatherObservationId(long value) => Value = value;

        public static WeatherObservationId From(long value)
        {
            if (value <= 0)
                throw new DomainValidationException(ErrorCodes.InvalidId, "WeatherObservationId must be positive.");
            return new WeatherObservationId(value);
        }

        public static WeatherObservationId Empty => new(0);

        public override string ToString() => Value.ToString();
    }
}
