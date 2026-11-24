using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Plot
{
    // خطاهای دامنه مشاهده هواشناسی — هم‌سبک PlotErrors (کد + پیام فارسی + جزئیات)
    public static class WeatherObservationErrors
    {
        public static Error NullReferenceError()
            => Error.Create(
                code: "Plot.WeatherObservationNullReference",
                message: "شناسه مشاهده هواشناسی یا قطعه زمین ارسال نشده است.");

        public static Error ObservedAtInFuture(DateTime observedAt)
            => Error.Create(
                code: "Plot.WeatherObservedAtInFuture",
                message: "زمان مشاهده هواشناسی نمی‌تواند در آینده باشد.",
                details: new Dictionary<string, object>
                {
                    ["ObservedAt"] = observedAt.ToString("yyyy-MM-dd HH:mm")
                });

        public static Error MeasurementRequired()
            => Error.Create(
                code: "Plot.WeatherMeasurementRequired",
                message: "برای ثبت مشاهده هواشناسی حداقل یکی از اندازه‌گیری‌ها (دما، رطوبت، بارش یا باد) لازم است.");

        public static Error NoteTooLong(int length)
            => Error.Create(
                code: "Plot.WeatherNoteTooLong",
                message: "یادداشت مشاهده هواشناسی نباید بیش از ۵۰۰ کاراکتر باشد.",
                details: new Dictionary<string, object>
                {
                    ["Length"] = length,
                    ["MaxLength"] = WeatherObservation.MaxNoteLength
                });

        public static Error NoteUnchanged()
            => Error.Create(
                code: "Plot.WeatherNoteUnchanged",
                message: "یادداشت جدید با یادداشت فعلی یکسان است.");
    }
}
