using Fartak.Domain.Framework;
using Fartak.Domain.Unit.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fartak.Domain.Unit
{
    // واحد اندازه‌گیری (جدول units) — تجمع مستقل سبک، مدیریتی؛
    // آیتم‌های عددی چک‌لیست، خطوط طرح، پاس‌های عددی بازدید و خطوط خرید با UnitId به آن ارجاع می‌دهند.
    //
    // جدول units فقط چهار ستون دارد (code/title/dimension)؛ بدون IsActive و بدون timestamp —
    // بنابراین این تجمع عمداً بدون متد فعال‌سازی و بدون رویداد وضعیت است.
    //
    // قاعده کد: کد واحد شناسه‌ای معنایی است (در seed و نمایش‌ها استفاده می‌شود) و
    // پس از ایجاد تغییر نمی‌کند — فقط عنوان و Dimension قابل ویرایش‌اند.
    public class Unit : AggregateRoot<UnitId>
    {
        public const int MaxCodeLength = 20;
        public const int MaxTitleLength = 60;
        public const int MaxDimensionLength = 30;

        public string Code { get; private set; }        // یکتا (unique) — مثل kg / lit / ha / mch
        public string Title { get; private set; }       // عنوان نمایشی فارسی مثل «کیلوگرم»
        public string? Dimension { get; private set; }  // بُعد اندازه‌گیری: وزن/حجم/مساحت/شمارش/زمان-ماشین/زمان-نیرو/طول

        private Unit() { } // برای EF Core

        private Unit(UnitId id, string code, string title, string? dimension)
        {
            Id = id;
            Code = code;
            Title = title;
            Dimension = dimension;
        }

        public static Result<Unit> Create(
            UnitId id,
            string code,
            string title,
            string? dimension = null)
        {
            if (id is null)
                return Result.Failure<Unit>(UnitErrors.NullReferenceError());

            var codeValidation = ValidateCode(code);
            if (codeValidation.IsFailure)
                return Result.Failure<Unit>(codeValidation.Error);

            var titleValidation = ValidateTitle(title);
            if (titleValidation.IsFailure)
                return Result.Failure<Unit>(titleValidation.Error);

            var dimensionValidation = ValidateDimension(dimension);
            if (dimensionValidation.IsFailure)
                return Result.Failure<Unit>(dimensionValidation.Error);

            var unit = new Unit(id, code.Trim(), title.Trim(), NormalizeDimension(dimension));

            unit.AddDomainEvent(new UnitCreatedEvent(id.Value, unit.Code, unit.Title, unit.Dimension));
            return Result<Unit>.Success(unit);
        }

        // تغییر عنوان نمایشی واحد (فرم ویرایش) — کد واحد تغییر نمی‌کند
        public Result Rename(string newTitle)
        {
            var validation = ValidateTitle(newTitle);
            if (validation.IsFailure)
                return validation;

            var trimmed = newTitle.Trim();
            if (trimmed == Title)
                return Result.Failure(UnitErrors.TitleUnchanged());

            var oldTitle = Title;
            Title = trimmed;
            AddDomainEvent(new UnitRenamedEvent(Id.Value, Code, oldTitle, Title));
            return Result.Success();
        }

        // تغییر بُعد اندازه‌گیری — مقدار تهی یعنی پاک‌کردن بُعد (فیلد اختیاری است)
        public Result SetDimension(string? newDimension)
        {
            var validation = ValidateDimension(newDimension);
            if (validation.IsFailure)
                return validation;

            var normalized = NormalizeDimension(newDimension);
            if (normalized == Dimension)
                return Result.Failure(UnitErrors.DimensionUnchanged());

            var oldDimension = Dimension;
            Dimension = normalized;
            AddDomainEvent(new UnitDimensionChangedEvent(Id.Value, Code, oldDimension, Dimension));
            return Result.Success();
        }

        // بُعدهای شناخته‌شده سیستم مطابق یادداشت ERD — برای جلوگیری از رشته‌های پراکنده در کل کد؛
        // عضویت اجباری نیست (فیلد آزاد است) تا افزودن بُعد جدید نیازمند تغییر کد نباشد
        public static class WellKnownDimensions
        {
            public const string Weight = "وزن";
            public const string Volume = "حجم";
            public const string Area = "مساحت";
            public const string Count = "شمارش";
            public const string MachineTime = "زمان-ماشین";
            public const string LaborTime = "زمان-نیرو";
            public const string Length = "طول";
        }

        private static string? NormalizeDimension(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static Result ValidateCode(string? code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return Result.Failure(UnitErrors.InvalidCode(code));

            if (code.Trim().Length > MaxCodeLength)
                return Result.Failure(UnitErrors.CodeTooLong(code.Trim()));

            return Result.Success();
        }

        private static Result ValidateTitle(string? title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return Result.Failure(UnitErrors.InvalidTitle(title));

            if (title.Trim().Length > MaxTitleLength)
                return Result.Failure(UnitErrors.TitleTooLong(title.Trim()));

            return Result.Success();
        }

        private static Result ValidateDimension(string? dimension)
        {
            if (string.IsNullOrWhiteSpace(dimension))
                return Result.Success(); // اختیاری است

            if (dimension.Trim().Length > MaxDimensionLength)
                return Result.Failure(UnitErrors.DimensionTooLong(dimension.Trim()));

            return Result.Success();
        }
    }
}
