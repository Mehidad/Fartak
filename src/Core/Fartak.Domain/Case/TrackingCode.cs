using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Case
{
    // کد رهگیری پرونده — معادل ستون tracking_code varchar(20) با قید unique
    // قالب: «FT-{سال جلالی}-{شماره ترتیبی صفرپرشده}» مثل FT-1403-02867
    // تولید شماره از جدول tracking_sequences انجام می‌شود (در لایه Application)؛
    // این VO فقط اعتبار ساختاری را تضمین می‌کند.
    public sealed class TrackingCode : ValueObject
    {
        public const int MaxLength = 20;
        public const string Prefix = "FT";
        public const int MinYear = 1380;
        public const int MaxYear = 1450;

        public string Value { get; }

        private TrackingCode() { } // برای EF Core

        private TrackingCode(string value) => Value = value;

        public static TrackingCode Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainValidationException(
                    "Case.InvalidTrackingCode",
                    "کد رهگیری پرونده ارسال نشده است.");

            var normalized = value.Trim().ToUpperInvariant();

            if (normalized.Length > MaxLength)
                throw new DomainValidationException(
                    "Case.TrackingCodeTooLong",
                    "کد رهگیری پرونده نباید بیش از ۲۰ کاراکتر باشد.",
                    new Dictionary<string, object> { ["Value"] = value });

            // سه بخش با خط تیره: FT / سال چهاررقمی جلالی / شماره ترتیبی ۱ تا ۶ رقمی
            var parts = normalized.Split('-');
            if (parts.Length != 3
                || parts[0] != Prefix
                || parts[1].Length != 4 || !IsAsciiDigits(parts[1])
                || parts[2].Length < 1 || parts[2].Length > 6 || !IsAsciiDigits(parts[2]))
                throw new DomainValidationException(
                    "Case.InvalidTrackingCode",
                    "قالب کد رهگیری صحیح نیست؛ نمونه معتبر: FT-1403-02867",
                    new Dictionary<string, object> { ["Value"] = value });

            var year = int.Parse(parts[1]);
            if (year < MinYear || year > MaxYear)
                throw new DomainValidationException(
                    "Case.InvalidTrackingCode",
                    $"سال داخل کد رهگیری باید بین {MinYear} و {MaxYear} باشد.",
                    new Dictionary<string, object> { ["Value"] = value });

            return new TrackingCode(normalized);
        }

        private static bool IsAsciiDigits(string text)
            => text.All(c => c is >= '0' and <= '9');

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;
    }
}
