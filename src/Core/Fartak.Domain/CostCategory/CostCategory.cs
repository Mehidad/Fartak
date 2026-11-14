using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.CostCategory
{
    // سرفصل هزینه (جدول cost_categories) — درخت سرفصل‌ها با خودارجاع parent_id؛
    // ردیف‌های هزینه (cost_entries) و تخصیص‌های آینده با CostCategoryId به آن ارجاع می‌دهند.
    //
    // جدول شش ستون دارد و بدون timestamp است — بنابراین این تجمع عمداً بدون CreatedAt/UpdatedAt است.
    //
    // قاعده کد: کد سرفصل کلید معنایی ارجاع است (unique در ERD) و پس از ایجاد تغییر نمی‌کند.
    //
    // قواعد درخت: والد همیشه با شناسه ارجاع می‌شود (نه مالکیت) تا کل درخت در حافظه بارگذاری نشود؛
    // خودارجاع در دامنه رد می‌شود؛ وجود والد، فعال‌بودن آن و تشخیص چرخه‌های چندمرحله‌ای (A→B→A)
    // با پارامتر parentIsActive از سوی لایه Application تأیید می‌شود (الگوی parentLevel در Region).
    // غیرفعال‌سازی والد، فرزندانِ موجود را تغییر نمی‌دهد؛ فقط تعریف/انتقال فرزند جدید زیر آن مسدود می‌شود.
    public class CostCategory : AggregateRoot<CostCategoryId>
    {
        public const int MaxCodeLength = 40;    // varchar(40) در ERD
        public const int MaxTitleLength = 120;  // varchar(120) در ERD
        public const int MaxSortOrder = 32767;  // سقف smallint مطابق ERD

        public string Code { get; private set; }               // یکتا — مثل fertilizer / fuel / labor
        public string Title { get; private set; }              // عنوان نمایشی فارسی مثل «کود و نهاده»
        public CostCategoryId? ParentId { get; private set; }  // والد؛ سرفصل‌های ریشه بدون والد
        public int SortOrder { get; private set; }             // ترتیب نمایش در درخت/کامبوها
        public bool IsActive { get; private set; }

        private CostCategory() { } // برای EF Core

        private CostCategory(CostCategoryId id, string code, string title, CostCategoryId? parentId, int sortOrder)
        {
            Id = id;
            Code = code;
            Title = title;
            ParentId = parentId;
            SortOrder = sortOrder;
            IsActive = true;
        }

        public static Result<CostCategory> Create(
            CostCategoryId id,
            string code,
            string title,
            CostCategoryId? parentId = null,
            bool? parentIsActive = null,
            int sortOrder = 0)
        {
            if (id is null)
                return Result.Failure<CostCategory>(CostCategoryErrors.NullReferenceError());

            var parentValidation = ValidateParent(id, parentId, parentIsActive);
            if (parentValidation.IsFailure)
                return Result.Failure<CostCategory>(parentValidation.Error);

            var codeValidation = ValidateCode(code);
            if (codeValidation.IsFailure)
                return Result.Failure<CostCategory>(codeValidation.Error);

            var titleValidation = ValidateTitle(title);
            if (titleValidation.IsFailure)
                return Result.Failure<CostCategory>(titleValidation.Error);

            var orderValidation = ValidateSortOrder(sortOrder);
            if (orderValidation.IsFailure)
                return Result.Failure<CostCategory>(orderValidation.Error);

            var category = new CostCategory(id, code.Trim(), title.Trim(), parentId, sortOrder);
            category.AddDomainEvent(new CostCategoryCreatedEvent(
                id.Value, category.Code, category.Title, parentId?.Value, sortOrder));
            return Result<CostCategory>.Success(category);
        }

        // تغییر عنوان نمایشی سرفصل — کد و جایگاه در درخت تغییر نمی‌کنند
        public Result Rename(string newTitle)
        {
            var validation = ValidateTitle(newTitle);
            if (validation.IsFailure)
                return validation;

            var trimmed = newTitle.Trim();
            if (trimmed == Title)
                return Result.Failure(CostCategoryErrors.TitleUnchanged());

            var oldTitle = Title;
            Title = trimmed;
            AddDomainEvent(new CostCategoryRenamedEvent(Id.Value, Code, oldTitle, Title));
            return Result.Success();
        }

        // جابه‌جایی سرفصل به والد دیگر (انتقال شاخه) — بدون والد یعنی تبدیل به ریشه؛
        // وضعیت فعالِ والدِ جدید از سوی لایه Application تأیید می‌شود
        public Result ChangeParent(CostCategoryId? newParentId, bool? newParentIsActive)
        {
            var parentValidation = ValidateParent(Id, newParentId, newParentIsActive);
            if (parentValidation.IsFailure)
                return parentValidation;

            if (ParentId?.Value == newParentId?.Value)
                return Result.Failure(CostCategoryErrors.ParentUnchanged());

            var oldParentId = ParentId;
            ParentId = newParentId;
            AddDomainEvent(new CostCategoryParentChangedEvent(Id.Value, Code, oldParentId?.Value, newParentId?.Value));
            return Result.Success();
        }

        // تغییر ترتیب نمایش سرفصل (جابه‌جایی در درخت/کامبوها)
        public Result SetSortOrder(int newSortOrder)
        {
            var validation = ValidateSortOrder(newSortOrder);
            if (validation.IsFailure)
                return validation;

            if (newSortOrder == SortOrder)
                return Result.Failure(CostCategoryErrors.SortOrderUnchanged());

            var oldOrder = SortOrder;
            SortOrder = newSortOrder;
            AddDomainEvent(new CostCategorySortOrderChangedEvent(Id.Value, Code, oldOrder, newSortOrder));
            return Result.Success();
        }

        public Result Activate()
        {
            if (IsActive)
                return Result.Failure(CostCategoryErrors.AlreadyActive(Id.Value));

            IsActive = true;
            AddDomainEvent(new CostCategoryActivatedEvent(Id.Value, Code));
            return Result.Success();
        }

        // غیرفعال‌سازی فقط یعنی حذف از کامبوهای انتخاب سرفصل جدید؛
        // ارجاع‌های CostCategoryId ذخیره‌شده در ردیف‌های هزینه معتبر می‌مانند.
        // بررسی «این سرفصل فرزند فعال دارد؟» نیازمند کل درخت است و در لایه Application انجام می‌شود
        public Result Deactivate()
        {
            if (!IsActive)
                return Result.Failure(CostCategoryErrors.AlreadyDeactivated(Id.Value));

            IsActive = false;
            AddDomainEvent(new CostCategoryDeactivatedEvent(Id.Value, Code));
            return Result.Success();
        }

        // قاعده مشترک Create و ChangeParent — ساختار درخت:
        // خودارجاع ممنوع؛ اگر والد اعلام شده، وضعیت فعال آن باید صریحاً تأیید شده باشد
        private static Result ValidateParent(CostCategoryId id, CostCategoryId? parentId, bool? parentIsActive)
        {
            if (parentId is null)
                return Result.Success();

            if (parentId.Value.Value == id.Value)
                return Result.Failure(CostCategoryErrors.SelfParent());

            if (parentIsActive is null)
                return Result.Failure(CostCategoryErrors.ParentStateNotProvided());

            if (!parentIsActive.Value)
                return Result.Failure(CostCategoryErrors.InactiveParent());

            return Result.Success();
        }

        private static Result ValidateCode(string? code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return Result.Failure(CostCategoryErrors.InvalidCode(code));

            if (code.Trim().Length > MaxCodeLength)
                return Result.Failure(CostCategoryErrors.CodeTooLong(code.Trim()));

            return Result.Success();
        }

        private static Result ValidateTitle(string? title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return Result.Failure(CostCategoryErrors.InvalidTitle(title));

            if (title.Trim().Length > MaxTitleLength)
                return Result.Failure(CostCategoryErrors.TitleTooLong(title.Trim()));

            return Result.Success();
        }

        private static Result ValidateSortOrder(int sortOrder)
        {
            if (sortOrder < 0 || sortOrder > MaxSortOrder)
                return Result.Failure(CostCategoryErrors.SortOrderOutOfRange(sortOrder));

            return Result.Success();
        }
    }
}
