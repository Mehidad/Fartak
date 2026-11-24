using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;
using Fartak.Domain.User;

namespace Fartak.Domain.Plot
{
    // مشاهده هواشناسی قطعه زمین — موجودیت فرزندِ Plot (جدول weather_observations)
    // فقط از طریق خود Aggregate ساخته می‌شود: Plot.RecordWeatherObservation
    //
    // append-only: رکوردها پس از ثبت اصلاح نمی‌شوند (سابقهٔ تاریخی هوا)؛
    // تنها ویرایش مجاز یادداشت آزاد است (SetNote).
    // طبق قاعده timestamp/is_active: بدون updated_at → بدون UpdatedAt؛
    // بدون is_active → بدون Activate/Deactivate.
    //
    // آستانه‌های هشدار زراعی const تعریف شده‌اند تا قضاوت «سرمازدگی / تنش گرمایی /
    // بارش سنگین / باد شدید» فقط از همین مرز تجمع بیرون بیاید و Handlerهای
    // لایه Application (اعلان به کشاورز) همین ثابت‌ها را بخوانند.
    public sealed class WeatherObservation : Entity<WeatherObservationId>
    {
        public const int MaxNoteLength = 500;

        // آستانه‌های هشدار زراعی
        public const double FrostTemperatureC = 0d;        // سرمازدگی: دما ≤ ۰ درجه سلسیوس
        public const double HeatStressTemperatureC = 35d;  // تنش گرمایی: دما ≥ ۳۵ درجه سلسیوس
        public const double HeavyRainfallMm = 30d;         // بارش سنگین: بارش ≥ ۳۰ میلی‌متر
        public const double StrongWindSpeedMs = 15d;       // باد شدید: سرعت باد ≥ ۱۵ متر بر ثانیه

        public PlotId PlotId { get; private set; }             // قطعه زمین صاحب رکورد
        public DateTime ObservedAt { get; private set; }       // زمان مشاهده
        public double? TemperatureC { get; private set; }      // دما (سلسیوس)
        public double? HumidityPercent { get; private set; }   // رطوبت نسبی (درصد)
        public double? RainfallMm { get; private set; }        // بارش (میلی‌متر)
        public double? WindSpeedMs { get; private set; }       // سرعت باد (متر بر ثانیه)
        public string? Note { get; private set; }              // یادداشت آزاد — تنها فیلد قابل ویرایش
        public UserId? RecordedBy { get; private set; }        // ثبت‌کننده (NULL = سیستم)
        public DateTime CreatedAt { get; private set; }

        private WeatherObservation() { } // برای EF Core

        private WeatherObservation(
            WeatherObservationId id,
            PlotId plotId,
            DateTime observedAt,
            double? temperatureC,
            double? humidityPercent,
            double? rainfallMm,
            double? windSpeedMs,
            string? note,
            UserId? recordedBy)
        {
            Id = id;
            PlotId = plotId;
            ObservedAt = observedAt;
            TemperatureC = temperatureC;
            HumidityPercent = humidityPercent;
            RainfallMm = rainfallMm;
            WindSpeedMs = windSpeedMs;
            Note = note;
            RecordedBy = recordedBy;
            CreatedAt = DateTime.UtcNow;
        }

        // ساخت رکورد — فقط از طریق Plot.RecordWeatherObservation
        internal static Result<WeatherObservation> Create(
            WeatherObservationId id,
            PlotId plotId,
            DateTime observedAt,
            double? temperatureC,
            double? humidityPercent,
            double? rainfallMm,
            double? windSpeedMs,
            string? note,
            UserId? recordedBy)
        {
            if (id is null || plotId is null)
                return Result.Failure<WeatherObservation>(WeatherObservationErrors.NullReferenceError());

            if (observedAt > DateTime.UtcNow)
                return Result.Failure<WeatherObservation>(WeatherObservationErrors.ObservedAtInFuture(observedAt));

            var hasAnyMeasurement = temperatureC is not null
                || humidityPercent is not null
                || rainfallMm is not null
                || windSpeedMs is not null;
            if (!hasAnyMeasurement)
                return Result.Failure<WeatherObservation>(WeatherObservationErrors.MeasurementRequired());

            var trimmedNote = NormalizeNote(note);
            if (trimmedNote is not null && trimmedNote.Length > MaxNoteLength)
                return Result.Failure<WeatherObservation>(WeatherObservationErrors.NoteTooLong(trimmedNote.Length));

            return Result<WeatherObservation>.Success(new WeatherObservation(
                id, plotId, observedAt, temperatureC, humidityPercent, rainfallMm, windSpeedMs, trimmedNote, recordedBy));
        }

        // تنها ویرایش مجاز روی رکورد append-only؛ ارسال null/خالی یعنی پاک‌کردن یادداشت
        internal Result SetNote(string? note)
        {
            var trimmedNote = NormalizeNote(note);
            if (trimmedNote is not null && trimmedNote.Length > MaxNoteLength)
                return Result.Failure(WeatherObservationErrors.NoteTooLong(trimmedNote.Length));

            if (trimmedNote == Note)
                return Result.Failure(WeatherObservationErrors.NoteUnchanged());

            Note = trimmedNote;
            return Result.Success();
        }

        // هشدارهای زراعی بر اساس آستانه‌ها — مبنای اعلان‌های لایه Application
        public bool HasFrostRisk => TemperatureC is not null && TemperatureC.Value <= FrostTemperatureC;
        public bool HasHeatStress => TemperatureC is not null && TemperatureC.Value >= HeatStressTemperatureC;
        public bool HasHeavyRainfall => RainfallMm is not null && RainfallMm.Value >= HeavyRainfallMm;
        public bool HasStrongWind => WindSpeedMs is not null && WindSpeedMs.Value >= StrongWindSpeedMs;

        private static string? NormalizeNote(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
