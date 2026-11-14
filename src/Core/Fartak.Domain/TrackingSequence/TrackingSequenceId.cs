using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Case;
using Fartak.Domain.Framework;

namespace Fartak.Domain.TrackingSequence
{
    // شناسه یک ردیف از جدول tracking_sequences = سال جلالی (jalali_year int PK)
    //
    // استثنای عمدی نسبت به الگوی Snowflake: این جدول شمارنده سالانه است و یک ردیف به ازای هر سال دارد؛
    // بنابراین شناسه تجمع همان سال است (نه شناسه ردیف بزرگ‌ integer).
    // مرزهای معتبر سال از TrackingCode (پوشه Case) گرفته می‌شود تا منبع واحد حقیقت باشد.
    public sealed record TrackingSequenceId
    {
        public int Value { get; }

        private TrackingSequenceId(int value) => Value = value;

        public static TrackingSequenceId From(int value)
        {
            if (value < TrackingCode.MinYear || value > TrackingCode.MaxYear)
                throw new DomainValidationException(
                    ErrorCodes.InvalidId,
                    $"TrackingSequenceId must be a Jalali year between {TrackingCode.MinYear} and {TrackingCode.MaxYear}.");
            return new TrackingSequenceId(value);
        }

        public static TrackingSequenceId Empty => new(0);

        public override string ToString() => Value.ToString();
    }
}
