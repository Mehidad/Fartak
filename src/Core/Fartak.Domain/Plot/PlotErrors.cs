using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Plot
{
    public static class PlotErrors
    {
        public static Error NullReferenceError()
            => Error.Create(
                code: "Plot.NullReference",
                message: "مقادیر اجباری قطعه زمین ارسال نشده است.");

        public static Error InvalidTitle(string? title)
            => Error.Create(
                code: "Plot.InvalidTitle",
                message: "عنوان قطعه زمین نامعتبر است.",
                details: new Dictionary<string, object>
                {
                    ["Title"] = title ?? string.Empty
                });

        public static Error TitleTooLong(string title)
            => Error.Create(
                code: "Plot.TitleTooLong",
                message: "عنوان قطعه زمین نباید بیش از ۱۲۰ کاراکتر باشد.",
                details: new Dictionary<string, object>
                {
                    ["Title"] = title
                });

        public static Error InvalidGeoJson()
            => Error.Create(
                code: "Plot.InvalidGeoJson",
                message: "ساختار GeoJSON ارسالی برای پلی‌گان زمین نامعتبر است.");

        public static Error ComputedAreaRequiresGeoJson()
            => Error.Create(
                code: "Plot.ComputedAreaRequiresGeoJson",
                message: "مساحت «محاسبه‌شده» بدون پلی‌گان GeoJSON امکان‌پذیر نیست؛ یا پلی‌گان ارسال کنید یا منبع مساحت را دستی (Manual) انتخاب کنید.");

        public static Error InvalidSoilTexture(string soilTexture)
            => Error.Create(
                code: "Plot.InvalidSoilTexture",
                message: "نوع خاک نباید بیش از ۶۰ کاراکتر باشد.",
                details: new Dictionary<string, object>
                {
                    ["SoilTexture"] = soilTexture
                });

        public static Error InvalidLabName(string labName)
            => Error.Create(
                code: "Plot.InvalidLabName",
                message: "نام آزمایشگاه نباید بیش از ۱۲۰ کاراکتر باشد.",
                details: new Dictionary<string, object>
                {
                    ["LabName"] = labName
                });

        public static Error SoilTestDateInFuture(DateOnly testedAt)
            => Error.Create(
                code: "Plot.SoilTestDateInFuture",
                message: "تاریخ تست خاک نمی‌تواند در آینده باشد.",
                details: new Dictionary<string, object>
                {
                    ["TestedAt"] = testedAt.ToString("yyyy-MM-dd")
                });

        public static Error PlotNotActiveError(long plotId)
            => Error.Create(
                code: "Plot.NotActive",
                message: "قطعه زمین غیرفعال است و این عملیات امکان‌پذیر نیست.",
                details: new Dictionary<string, object>
                {
                    ["PlotId"] = plotId
                });

        public static Error PlotAlreadyActiveError(long plotId)
            => Error.Create(
                code: "Plot.AlreadyActive",
                message: "قطعه زمین از قبل فعال است.",
                details: new Dictionary<string, object>
                {
                    ["PlotId"] = plotId
                });

        public static Error PlotAlreadyDeactivatedError(long plotId)
            => Error.Create(
                code: "Plot.AlreadyDeactivated",
                message: "قطعه زمین از قبل غیرفعال شده است.",
                details: new Dictionary<string, object>
                {
                    ["PlotId"] = plotId
                });
    }
}
