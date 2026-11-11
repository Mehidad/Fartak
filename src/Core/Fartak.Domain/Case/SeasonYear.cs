using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Case
{
    // سال زراعی جلالی — معادل ستون season_year int
    // «فصل» پرونده است؛ پرونده روی «یک قطعه × یک محصول × یک سال زراعی» باز می‌شود.
    public sealed class SeasonYear : ValueObject
    {
        public const int MinYear = 1380; // شروع داده‌های معتبر فرتاک
        public const int MaxYear = 1450;

        public int Year { get; }

        private SeasonYear() { } // برای EF Core

        private SeasonYear(int year) => Year = year;

        public static SeasonYear Create(int year)
        {
            if (year < MinYear || year > MaxYear)
                throw new DomainValidationException(
                    "Case.InvalidSeasonYear",
                    $"سال زراعی باید بین {MinYear} و {MaxYear} باشد.",
                    new Dictionary<string, object> { ["Year"] = year });

            return new SeasonYear(year);
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Year;
        }

        public override string ToString() => Year.ToString();
    }
}
