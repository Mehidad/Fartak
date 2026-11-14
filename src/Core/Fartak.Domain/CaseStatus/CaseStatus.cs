using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Fartak.Domain.Case;
using Fartak.Domain.Framework;

namespace Fartak.Domain.CaseStatus
{

    // وضعیت چرخه پرونده (جدول case_statuses) + گذارهای خروجی آن (جدول case_status_transitions)
    // — وضعیت‌ها داده مرجعِ قابل تنظیم هستند (نه enum سخت) و ماشین وضعیت با همین تعاریف کار می‌کند.
    //
    // تقسیم کار با لایه Application: تعریف گذارها در همین تجمع نگه داشته می‌شود؛
    // «آیا پرونده می‌تواند الان به وضعیت X برود؟» با بارگذاری همین تجمع و بررسی گذارها
    // در لایه Application پاسخ داده می‌شود و ثبت case_status_logs هم همان‌جاست.
    //
    // قواعد تغییرناپذیری: کد وضعیت کلید ارجاع است (unique در ERD)؛ group_code هم جایگاه وضعیت
    // در چرخه پرونده را تعیین می‌کند و تغییر آن معنای لاگ‌های تاریخی را می‌شکند — هر دو متد تغییر ندارند.
    //
    // قاعده وضعیت پایانی: از وضعیت پایانی گذار خروجی تعریف نمی‌شود و وضعیتی که گذار خروجی دارد
    // نمی‌تواند پایانی اعلام شود (هر دو داخل مرز تجمع کنترل می‌شود).
    //
    // نقش‌های مجاز گذار: فهرست کد نقش‌ها (مثل expert/admin)؛ فهرست خالی یعنی گذار فقط توسط سیستم؛
    // وجود کد نقش در جدول roles در لایه Application بررسی می‌شود.
    //
    // حذف فیزیکی گذار مجاز است؛ case_status_logs به وضعیت‌ها ارجاع دارند نه به گذارها.
    // هیچ‌کدام از دو جدول ستون timestamp یا is_active ندارند — بنابراین این تجمع عمداً
    // بدون CreatedAt/UpdatedAt و بدون متد فعال‌سازی است.
    public class CaseStatus : AggregateRoot<CaseStatusId>
    {
        public const int MaxCodeLength = 40;       // varchar(40) در ERD
        public const int MaxTitleLength = 120;     // varchar(120) در ERD
        public const int MaxGroupCodeLength = 30;  // varchar(30) در ERD
        public const int MaxColorLength = 10;      // varchar(10) در ERD — مثل #RRGGBB یا نام رنگ
        public const int MaxSortOrder = 32767;     // سقف smallint مطابق ERD

        public string Code { get; private set; }        // یکتا — مثل draft / expert_review / completed
        public string Title { get; private set; }       // عنوان نمایشی فارسی مثل «در حال بررسی کارشناس»
        public string GroupCode { get; private set; }   // گروه چرخه: intake/expert/planning/.../final — immutable
        public int SortOrder { get; private set; }      // ترتیب نمایش در کامبوها و نمودار وضعیت‌ها
        public bool IsTerminal { get; private set; }    // وضعیت پایانی؟ (completed / canceled / ...)
        public string? Color { get; private set; }      // رنگ نمایش در UI (اختیاری)

        private readonly List<CaseStatusTransition> _transitions = new();

        // گذارهای خروجی این وضعیت (from = همین وضعیت) — موجودیت‌های فرزند
        public IReadOnlyCollection<CaseStatusTransition> Transitions => _transitions.AsReadOnly();

        private CaseStatus() { } // برای EF Core

        private CaseStatus(CaseStatusId id, string code, string title, string groupCode, int sortOrder, string? color)
        {
            Id = id;
            Code = code;
            Title = title;
            GroupCode = groupCode;
            SortOrder = sortOrder;
            IsTerminal = false;
            Color = color;
        }

        public static Result<CaseStatus> Create(
            CaseStatusId id,
            string code,
            string title,
            string groupCode,
            int sortOrder = 0,
            string? color = null)
        {
            if (id is null)
                return Result.Failure<CaseStatus>(CaseStatusErrors.NullReferenceError());

            var codeValidation = ValidateCode(code);
            if (codeValidation.IsFailure)
                return Result.Failure<CaseStatus>(codeValidation.Error);

            var titleValidation = ValidateTitle(title);
            if (titleValidation.IsFailure)
                return Result.Failure<CaseStatus>(titleValidation.Error);

            var groupValidation = ValidateGroupCode(groupCode);
            if (groupValidation.IsFailure)
                return Result.Failure<CaseStatus>(groupValidation.Error);

            var orderValidation = ValidateSortOrder(sortOrder);
            if (orderValidation.IsFailure)
                return Result.Failure<CaseStatus>(orderValidation.Error);

            var colorValidation = ValidateColor(color);
            if (colorValidation.IsFailure)
                return Result.Failure<CaseStatus>(colorValidation.Error);

            var status = new CaseStatus(id, code.Trim(), title.Trim(), groupCode.Trim(), sortOrder, NormalizeColor(color));
            status.AddDomainEvent(new CaseStatusCreatedEvent(
                id.Value, status.Code, status.Title, status.GroupCode, status.SortOrder, status.Color));
            return Result<CaseStatus>.Success(status);
        }

        // تغییر عنوان نمایشی وضعیت — کد و گروه چرخه تغییر نمی‌کنند
        public Result Rename(string newTitle)
        {
            var validation = ValidateTitle(newTitle);
            if (validation.IsFailure)
                return validation;

            var trimmed = newTitle.Trim();
            if (trimmed == Title)
                return Result.Failure(CaseStatusErrors.TitleUnchanged());

            var oldTitle = Title;
            Title = trimmed;
            AddDomainEvent(new CaseStatusRenamedEvent(Id.Value, Code, oldTitle, Title));
            return Result.Success();
        }

        // تغییر ترتیب نمایش وضعیت
        public Result SetSortOrder(int newSortOrder)
        {
            var validation = ValidateSortOrder(newSortOrder);
            if (validation.IsFailure)
                return validation;

            if (newSortOrder == SortOrder)
                return Result.Failure(CaseStatusErrors.SortOrderUnchanged());

            var oldOrder = SortOrder;
            SortOrder = newSortOrder;
            AddDomainEvent(new CaseStatusSortOrderChangedEvent(Id.Value, Code, oldOrder, newSortOrder));
            return Result.Success();
        }

        // تعیین/تغییر/پاک‌کردن رنگ نمایش وضعیت — مقدار تهی یعنی بدون رنگ اختصاصی
        public Result SetColor(string? newColor)
        {
            var validation = ValidateColor(newColor);
            if (validation.IsFailure)
                return validation;

            var normalized = NormalizeColor(newColor);
            if (normalized == Color)
                return Result.Failure(CaseStatusErrors.ColorUnchanged());

            var oldColor = Color;
            Color = normalized;
            AddDomainEvent(new CaseStatusColorChangedEvent(Id.Value, Code, oldColor, Color));
            return Result.Success();
        }

        // اعلام/لغو وضعیت پایانی — وضعیتی که گذار خروجی دارد نمی‌تواند پایانی شود
        public Result SetTerminality(bool isTerminal)
        {
            if (isTerminal == IsTerminal)
                return Result.Failure(CaseStatusErrors.TerminalityUnchanged());

            if (isTerminal && _transitions.Count > 0)
                return Result.Failure(CaseStatusErrors.TerminalStatusWithOutgoingTransitions());

            IsTerminal = isTerminal;
            AddDomainEvent(new CaseStatusTerminalityChangedEvent(Id.Value, Code, IsTerminal));
            return Result.Success();
        }

        // تعریف گذار خروجی به وضعیت مقصد — UNIQUE (from,to) داخل مرز تجمع حفظ می‌شود؛
        // وجود وضعیت مقصد در جدول case_statuses از سوی لایه Application تضمین می‌شود
        public Result AddTransition(
            CaseStatusTransitionId transitionId,
            CaseStatusId toStatusId,
            IEnumerable<string>? allowedRoleCodes,
            bool isAutomatic,
            string? note)
        {
            if (transitionId is null || toStatusId is null)
                return Result.Failure(CaseStatusErrors.TransitionNullReference());

            if (IsTerminal)
                return Result.Failure(CaseStatusErrors.AddTransitionFromTerminalStatus());

            if (toStatusId.Value == Id.Value)
                return Result.Failure(CaseStatusErrors.SelfTransition());

            if (_transitions.Any(t => t.ToStatusId.Value == toStatusId.Value))
                return Result.Failure(CaseStatusErrors.TransitionAlreadyExists(toStatusId.Value));

            var rolesValidation = ValidateRoleCodes(allowedRoleCodes);
            if (rolesValidation.IsFailure)
                return rolesValidation;

            var noteValidation = ValidateNote(note);
            if (noteValidation.IsFailure)
                return noteValidation;

            var transition = CaseStatusTransition.Create(
                transitionId, toStatusId, NormalizeRoleCodes(allowedRoleCodes), isAutomatic, NormalizeNote(note));
            _transitions.Add(transition);

            AddDomainEvent(new CaseStatusTransitionAddedEvent(
                Id.Value, Code, transition.Id.Value, toStatusId.Value, transition.IsAutomatic));
            return Result.Success();
        }

        // تغییر فهرست نقش‌های مجاز یک گذار — فهرست خالی یعنی گذار فقط توسط سیستم
        public Result SetTransitionRoles(CaseStatusTransitionId transitionId, IEnumerable<string>? allowedRoleCodes)
        {
            var transition = FindTransition(transitionId);
            if (transition is null)
                return Result.Failure(CaseStatusErrors.TransitionNotFound(transitionId?.Value ?? 0));

            var rolesValidation = ValidateRoleCodes(allowedRoleCodes);
            if (rolesValidation.IsFailure)
                return rolesValidation;

            var normalized = NormalizeRoleCodes(allowedRoleCodes);
            if (normalized.SequenceEqual(transition.AllowedRoleCodes, StringComparer.Ordinal))
                return Result.Failure(CaseStatusErrors.TransitionRolesUnchanged());

            transition.SetRoles(normalized);
            AddDomainEvent(new CaseStatusTransitionRolesChangedEvent(
                Id.Value, Code, transition.Id.Value, transition.ToStatusId.Value));
            return Result.Success();
        }

        // تغییر خودکار/دستی‌بودن گذار — گذار خودکار بدون دخالت نقش‌ها توسط سیستم انجام می‌شود
        public Result SetTransitionAutomaticity(CaseStatusTransitionId transitionId, bool isAutomatic)
        {
            var transition = FindTransition(transitionId);
            if (transition is null)
                return Result.Failure(CaseStatusErrors.TransitionNotFound(transitionId?.Value ?? 0));

            if (isAutomatic == transition.IsAutomatic)
                return Result.Failure(CaseStatusErrors.TransitionAutomaticityUnchanged());

            transition.SetAutomaticity(isAutomatic);
            AddDomainEvent(new CaseStatusTransitionAutomaticityChangedEvent(
                Id.Value, Code, transition.Id.Value, transition.ToStatusId.Value, isAutomatic));
            return Result.Success();
        }

        // تغییر یادداشت توضیحی گذار — مقدار تهی یعنی پاک‌کردن یادداشت
        public Result SetTransitionNote(CaseStatusTransitionId transitionId, string? note)
        {
            var transition = FindTransition(transitionId);
            if (transition is null)
                return Result.Failure(CaseStatusErrors.TransitionNotFound(transitionId?.Value ?? 0));

            var noteValidation = ValidateNote(note);
            if (noteValidation.IsFailure)
                return noteValidation;

            var normalized = NormalizeNote(note);
            if (normalized == transition.Note)
                return Result.Failure(CaseStatusErrors.TransitionNoteUnchanged());

            transition.SetNote(normalized);
            AddDomainEvent(new CaseStatusTransitionNoteChangedEvent(
                Id.Value, Code, transition.Id.Value, transition.ToStatusId.Value));
            return Result.Success();
        }

        // حذف فیزیکی گذار — ارجاعی از بیرون به گذار وجود ندارد (لاگ‌ها به وضعیت ارجاع می‌دهند)
        public Result RemoveTransition(CaseStatusTransitionId transitionId)
        {
            var transition = FindTransition(transitionId);
            if (transition is null)
                return Result.Failure(CaseStatusErrors.TransitionNotFound(transitionId?.Value ?? 0));

            _transitions.Remove(transition);
            AddDomainEvent(new CaseStatusTransitionRemovedEvent(
                Id.Value, Code, transition.Id.Value, transition.ToStatusId.Value));
            return Result.Success();
        }

        // گروه‌های چرخه پرونده مطابق یادداشت ERD — برای جلوگیری از رشته‌های پراکنده در کل کد؛
        // عضویت اجباری نیست (فیلد آزاد ۳۰ نویسه‌ای است) تا افزودن گروه جدید نیازمند تغییر کد نباشد
        public static class WellKnownGroups
        {
            public const string Intake = "intake";           // پذیرش و ثبت اولیه
            public const string Expert = "expert";           // کارشناسی و بازدید
            public const string Planning = "planning";       // تدوین و تأیید برنامه
            public const string Execution = "execution";     // اجرای عملیات مزرعه
            public const string Harvest = "harvest";         // برداشت
            public const string Sale = "sale";               // فروش محصول
            public const string Settlement = "settlement";   // تسویه و مالی
            public const string Final = "final";             // بستن پرونده
        }

        private CaseStatusTransition? FindTransition(CaseStatusTransitionId? transitionId)
            => transitionId is null
                ? null
                : _transitions.FirstOrDefault(t => t.Id.Value == transitionId.Value);

        private static Result ValidateCode(string? code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return Result.Failure(CaseStatusErrors.InvalidCode(code));

            if (code.Trim().Length > MaxCodeLength)
                return Result.Failure(CaseStatusErrors.CodeTooLong(code.Trim()));

            return Result.Success();
        }

        private static Result ValidateTitle(string? title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return Result.Failure(CaseStatusErrors.InvalidTitle(title));

            if (title.Trim().Length > MaxTitleLength)
                return Result.Failure(CaseStatusErrors.TitleTooLong(title.Trim()));

            return Result.Success();
        }

        private static Result ValidateGroupCode(string? groupCode)
        {
            if (string.IsNullOrWhiteSpace(groupCode))
                return Result.Failure(CaseStatusErrors.InvalidGroupCode(groupCode));

            if (groupCode.Trim().Length > MaxGroupCodeLength)
                return Result.Failure(CaseStatusErrors.GroupCodeTooLong(groupCode.Trim()));

            return Result.Success();
        }

        private static Result ValidateSortOrder(int sortOrder)
        {
            if (sortOrder < 0 || sortOrder > MaxSortOrder)
                return Result.Failure(CaseStatusErrors.SortOrderOutOfRange(sortOrder));

            return Result.Success();
        }

        private static Result ValidateColor(string? color)
        {
            if (string.IsNullOrWhiteSpace(color))
                return Result.Success(); // اختیاری است

            if (color.Trim().Length > MaxColorLength)
                return Result.Failure(CaseStatusErrors.ColorTooLong(color.Trim()));

            return Result.Success();
        }

        // کد نقش‌ها: ورودی خالی/فاصله‌ای رد می‌شود؛ تکراری بودن هم رد می‌شود؛
        // سقف طول هر کد مطابق roles.code (varchar 40)؛ وجود کد در جدول roles در لایه Application
        private static Result ValidateRoleCodes(IEnumerable<string>? allowedRoleCodes)
        {
            if (allowedRoleCodes is null)
                return Result.Success(); // خالی = فقط سیستم

            foreach (var roleCode in allowedRoleCodes)
            {
                if (string.IsNullOrWhiteSpace(roleCode))
                    return Result.Failure(CaseStatusErrors.TransitionRoleCodeInvalid());

                if (roleCode.Trim().Length > CaseStatusTransition.MaxRoleCodeLength)
                    return Result.Failure(CaseStatusErrors.TransitionRoleCodeTooLong(roleCode.Trim()));
            }

            var duplicated = allowedRoleCodes
                .Select(c => c.Trim())
                .GroupBy(c => c, StringComparer.Ordinal)
                .FirstOrDefault(g => g.Count() > 1);
            if (duplicated is not null)
                return Result.Failure(CaseStatusErrors.TransitionRoleCodeDuplicated(duplicated.Key));

            return Result.Success();
        }

        private static Result ValidateNote(string? note)
        {
            if (string.IsNullOrWhiteSpace(note))
                return Result.Success(); // اختیاری است

            if (note.Trim().Length > CaseStatusTransition.MaxNoteLength)
                return Result.Failure(CaseStatusErrors.TransitionNoteTooLong(note.Trim()));

            return Result.Success();
        }

        private static string? NormalizeColor(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static string? NormalizeNote(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static List<string> NormalizeRoleCodes(IEnumerable<string>? allowedRoleCodes)
            => allowedRoleCodes is null
                ? new List<string>()
                : allowedRoleCodes.Select(c => c.Trim()).Distinct(StringComparer.Ordinal).ToList();
    }
}
