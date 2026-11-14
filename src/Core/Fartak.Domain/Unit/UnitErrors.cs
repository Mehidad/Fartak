using Fartak.Domain.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fartak.Domain.Unit
{
    public static class UnitErrors
    {
        public static Error NullReferenceError()
            => Error.Create(
                code: "Unit.NullReference",
                message: "مقادیر اجباری واحد اندازه‌گیری ارسال نشده است.");

        public static Error InvalidCode(string? code)
            => Error.Create(
                code: "Unit.InvalidCode",
                message: "کد واحد اندازه‌گیری نامعتبر است و نمی‌تواند خالی باشد.",
                details: new Dictionary<string, object>
                {
                    ["Code"] = code ?? string.Empty
                });

        public static Error CodeTooLong(string code)
            => Error.Create(
                code: "Unit.CodeTooLong",
                message: "کد واحد اندازه‌گیری نباید بیش از ۲۰ کاراکتر باشد.",
                details: new Dictionary<string, object>
                {
                    ["Code"] = code
                });

        public static Error InvalidTitle(string? title)
            => Error.Create(
                code: "Unit.InvalidTitle",
                message: "عنوان واحد اندازه‌گیری نامعتبر است و نمی‌تواند خالی باشد.",
                details: new Dictionary<string, object>
                {
                    ["Title"] = title ?? string.Empty
                });

        public static Error TitleTooLong(string title)
            => Error.Create(
                code: "Unit.TitleTooLong",
                message: "عنوان واحد اندازه‌گیری نباید بیش از ۶۰ کاراکتر باشد.",
                details: new Dictionary<string, object>
                {
                    ["Title"] = title
                });

        public static Error DimensionTooLong(string dimension)
            => Error.Create(
                code: "Unit.DimensionTooLong",
                message: "بُعد اندازه‌گیری نباید بیش از ۳۰ کاراکتر باشد.",
                details: new Dictionary<string, object>
                {
                    ["Dimension"] = dimension
                });

        public static Error TitleUnchanged()
            => Error.Create(
                code: "Unit.TitleUnchanged",
                message: "عنوان جدید همان عنوان فعلی واحد اندازه‌گیری است.");

        public static Error DimensionUnchanged()
            => Error.Create(
                code: "Unit.DimensionUnchanged",
                message: "بُعد جدید همان بُعد فعلی واحد اندازه‌گیری است.");
    }
}
