using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.PlanVersion
{
    public static class PlanVersionErrors
    {
        public static Error NullReferenceError()
            => Error.Create(
                code: "PlanVersion.NullReference",
                message: "مقادیر اجباری نسخه برنامه یا خط آن ارسال نشده است.");

        public static Error InvalidVersionNo(int versionNo)
            => Error.Create(
                code: "PlanVersion.InvalidVersionNo",
                message: "شماره نسخه برنامه باید عددی مثبت باشد.",
                details: new Dictionary<string, object>
                {
                    ["VersionNo"] = versionNo
                });

        public static Error InvalidInitialStatus(string statusCode)
            => Error.Create(
                code: "PlanVersion.InvalidInitialStatus",
                message: "نسخه جدید فقط با وضعیت «پیش‌نویس» (draft) ساخته می‌شود.",
                details: new Dictionary<string, object>
                {
                    ["InitialStatusCode"] = statusCode
                });

        public static Error UnknownPlanStatusCode(string statusCode)
            => Error.Create(
                code: "PlanVersion.UnknownPlanStatusCode",
                message: "کد وضعیت برای دامنه plan_status شناخته‌شده نیست؛ کدهای مجاز: draft، pending_approval، approved، rejected، superseded.",
                details: new Dictionary<string, object>
                {
                    ["StatusCode"] = statusCode
                });

        public static Error UnknownLineStatusCode(string statusCode)
            => Error.Create(
                code: "PlanVersion.UnknownLineStatusCode",
                message: "کد وضعیت خط برای دامنه line_status شناخته‌شده نیست؛ کدهای مجاز: planned، in_progress، done، delayed، skipped، canceled.",
                details: new Dictionary<string, object>
                {
                    ["StatusCode"] = statusCode
                });

        public static Error UnknownLineType(string lineType)
            => Error.Create(
                code: "PlanVersion.UnknownLineType",
                message: "نوع خط برنامه شناخته‌شده نیست؛ مقادیر مجاز: item (قلم)، action (اقدام).",
                details: new Dictionary<string, object>
                {
                    ["LineType"] = lineType
                });

        public static Error UnknownDecisionCode(string decisionCode)
            => Error.Create(
                code: "PlanVersion.UnknownDecisionCode",
                message: "کد تصمیم برای دامنه decision شناخته‌شده نیست؛ مقادیر مجاز: approved، rejected.",
                details: new Dictionary<string, object>
                {
                    ["DecisionCode"] = decisionCode
                });

        public static Error InvalidHarvestWindow(DateTime from, DateTime to)
            => Error.Create(
                code: "PlanVersion.InvalidHarvestWindow",
                message: "تاریخ پایان پنجره برداشت نمی‌تواند قبل از تاریخ شروع آن باشد.",
                details: new Dictionary<string, object>
                {
                    ["HarvestWindowFrom"] = from.ToString("yyyy-MM-dd"),
                    ["HarvestWindowTo"] = to.ToString("yyyy-MM-dd")
                });

        public static Error ExpectedYieldInvalid(decimal? value)
            => Error.Create(
                code: "PlanVersion.ExpectedYieldInvalid",
                message: "حداقل عملکرد موردانتظار در هر هکتار باید تا ۹٬۹۹۹٫۹۹ تن و با حداکثر دو رقم اعشار باشد.",
                details: new Dictionary<string, object>
                {
                    ["Value"] = value ?? 0
                });

        public static Error ExpectedTotalKgInvalid(decimal? value)
            => Error.Create(
                code: "PlanVersion.ExpectedTotalKgInvalid",
                message: "عملکرد کل موردانتظار باید تا ۹۹۹٬۹۹۹٬۹۹۹٬۹۹۹٫۹۹ کیلوگرم و با حداکثر دو رقم اعشار باشد.",
                details: new Dictionary<string, object>
                {
                    ["Value"] = value ?? 0
                });

        public static Error SummaryTooLong(int length)
            => Error.Create(
                code: "PlanVersion.SummaryTooLong",
                message: "متن خلاصه برنامه نباید بیش از ۴٬۰۰۰ کاراکتر باشد.",
                details: new Dictionary<string, object>
                {
                    ["Length"] = length
                });

        public static Error PlanInfoUnchanged()
            => Error.Create(
                code: "PlanVersion.PlanInfoUnchanged",
                message: "اطلاعات نسخه برنامه از قبل همین است.");

        public static Error VersionNotDraft(string currentStatusCode)
            => Error.Create(
                code: "PlanVersion.VersionNotDraft",
                message: "فقط نسخهٔ در وضعیت «پیش‌نویس» قابل ویرایش است؛ نسخه‌ها پس از ارسال برای تأیید منجمد می‌شوند و ویرایش یعنی نسخهٔ جدید.",
                details: new Dictionary<string, object>
                {
                    ["CurrentStatusCode"] = currentStatusCode
                });

        public static Error VersionNotPendingApproval(string currentStatusCode)
            => Error.Create(
                code: "PlanVersion.VersionNotPendingApproval",
                message: "این عملیات فقط در وضعیت «در انتظار تأیید» ممکن است.",
                details: new Dictionary<string, object>
                {
                    ["CurrentStatusCode"] = currentStatusCode
                });

        public static Error VersionNotApproved(string currentStatusCode)
            => Error.Create(
                code: "PlanVersion.VersionNotApproved",
                message: "ثبت اجرا و تغییر وضعیت خطوط فقط روی نسخهٔ «تأییدشده» ممکن است.",
                details: new Dictionary<string, object>
                {
                    ["CurrentStatusCode"] = currentStatusCode
                });

        public static Error EmptyPlanCannotSubmit()
            => Error.Create(
                code: "PlanVersion.EmptyPlanCannotSubmit",
                message: "برنامهٔ بدون خط قابل ارسال برای تأیید نیست.");

        public static Error ApprovalNotFound(long approvalId)
            => Error.Create(
                code: "PlanVersion.ApprovalNotFound",
                message: "ردیف تأیید در این نسخه یافت نشد.",
                details: new Dictionary<string, object>
                {
                    ["ApprovalId"] = approvalId
                });

        public static Error ApprovalDecisionAlreadyRecorded(long approvalId)
            => Error.Create(
                code: "PlanVersion.ApprovalDecisionAlreadyRecorded",
                message: "برای این نقش قبلاً تصمیم تأیید ثبت شده است.",
                details: new Dictionary<string, object>
                {
                    ["ApprovalId"] = approvalId
                });

        public static Error ApprovalCommentTooLong(int length)
            => Error.Create(
                code: "PlanVersion.ApprovalCommentTooLong",
                message: "متن توضیح تأیید نباید بیش از ۱٬۰۰۰ کاراکتر باشد.",
                details: new Dictionary<string, object>
                {
                    ["Length"] = length
                });

        public static Error PhaseNotFound(long phaseId)
            => Error.Create(
                code: "PlanVersion.PhaseNotFound",
                message: "فاز انتخاب‌شده در این نسخه از برنامه یافت نشد.",
                details: new Dictionary<string, object>
                {
                    ["PhaseId"] = phaseId
                });

        public static Error PhaseHasLines(long phaseId, int lineCount)
            => Error.Create(
                code: "PlanVersion.PhaseHasLines",
                message: "این فاز دارای خط برنامه است؛ ابتدا خطوط آن را حذف یا به فاز دیگری منتقل کنید.",
                details: new Dictionary<string, object>
                {
                    ["PhaseId"] = phaseId,
                    ["LineCount"] = lineCount
                });

        public static Error InvalidTitle()
            => Error.Create(
                code: "PlanVersion.InvalidTitle",
                message: "عنوان فاز یا خط برنامه خالی است.");

        public static Error TitleTooLong(int length)
            => Error.Create(
                code: "PlanVersion.TitleTooLong",
                message: "عنوان فاز یا خط برنامه نباید بیش از ۱۵۰ کاراکتر باشد.",
                details: new Dictionary<string, object>
                {
                    ["Length"] = length
                });

        public static Error InvalidQuantity()
            => Error.Create(
                code: "PlanVersion.InvalidQuantity",
                message: "مقدار خط برنامه باید بزرگ‌تر از صفر باشد.");

        public static Error QuantityTooPrecise()
            => Error.Create(
                code: "PlanVersion.QuantityTooPrecise",
                message: "مقدار خط برنامه حداکثر سه رقم اعشار می‌پذیرد.");

        public static Error QuantityOutOfRange()
            => Error.Create(
                code: "PlanVersion.QuantityOutOfRange",
                message: "مقدار خط برنامه از سقف مجاز numeric(12,3) بیشتر است.",
                details: new Dictionary<string, object>
                {
                    ["Max"] = 999_999_999.999m
                });

        public static Error CostCannotBeNegative()
            => Error.Create(
                code: "PlanVersion.CostCannotBeNegative",
                message: "هزینه نمی‌تواند منفی باشد.");

        public static Error InvalidPlannedDateRange(DateOnly plannedDate, DateOnly plannedEndDate)
            => Error.Create(
                code: "PlanVersion.InvalidPlannedDateRange",
                message: "تاریخ پایان برنامه‌ریزی نمی‌تواند قبل از تاریخ شروع باشد.",
                details: new Dictionary<string, object>
                {
                    ["PlannedDate"] = plannedDate.ToString("yyyy-MM-dd"),
                    ["PlannedEndDate"] = plannedEndDate.ToString("yyyy-MM-dd")
                });

        public static Error ExpertNoteTooLong(int length)
            => Error.Create(
                code: "PlanVersion.ExpertNoteTooLong",
                message: "یادداشت کارشناس روی خط نباید بیش از ۲٬۰۰۰ کاراکتر باشد.",
                details: new Dictionary<string, object>
                {
                    ["Length"] = length
                });

        public static Error SortOrderOutOfRange()
            => Error.Create(
                code: "PlanVersion.SortOrderOutOfRange",
                message: "ترتیب نمایش باید عددی بین ۰ تا ۳۲٬۷۶۷ باشد.");

        public static Error ItemRequiresCatalogItem()
            => Error.Create(
                code: "PlanVersion.ItemRequiresCatalogItem",
                message: "خط از نوع «قلم» باید به یک آیتم کاتالوگ متصل باشد.");

        public static Error ActionMustNotHaveCatalogItem()
            => Error.Create(
                code: "PlanVersion.ActionMustNotHaveCatalogItem",
                message: "خط از نوع «اقدام» نمی‌تواند آیتم کاتالوگ داشته باشد؛ برای اقدام، عنوان آزاد کافی است.");

        public static Error SupplyMethodOnlyForItem()
            => Error.Create(
                code: "PlanVersion.SupplyMethodOnlyForItem",
                message: "روش تأمین فقط برای خطوط از نوع «قلم» تعریف می‌شود.");

        public static Error PhaseTitleUnchanged()
            => Error.Create(
                code: "PlanVersion.PhaseTitleUnchanged",
                message: "عنوان فاز از قبل همین است.");

        public static Error PhaseSortOrderUnchanged()
            => Error.Create(
                code: "PlanVersion.PhaseSortOrderUnchanged",
                message: "ترتیب فاز از قبل همین است.");

        public static Error LineNotFound(long lineId)
            => Error.Create(
                code: "PlanVersion.LineNotFound",
                message: "خط برنامه در این نسخه یافت نشد.",
                details: new Dictionary<string, object>
                {
                    ["LineId"] = lineId
                });

        public static Error LineUnchanged()
            => Error.Create(
                code: "PlanVersion.LineUnchanged",
                message: "خط برنامه از قبل همین مقادیر را دارد.");

        public static Error LineStatusUnchanged(long lineId)
            => Error.Create(
                code: "PlanVersion.LineStatusUnchanged",
                message: "وضعیت خط از قبل همین است.",
                details: new Dictionary<string, object>
                {
                    ["LineId"] = lineId
                });

        public static Error InvalidLineStatusTransition(string fromStatusCode, string toStatusCode)
            => Error.Create(
                code: "PlanVersion.InvalidLineStatusTransition",
                message: "گذار وضعیت خط از وضعیت فعلی به وضعیت مقصد مجاز نیست؛ خطوط انجام‌شده، حذف‌شده و لغوشده پایانی‌اند.",
                details: new Dictionary<string, object>
                {
                    ["FromStatusCode"] = fromStatusCode,
                    ["ToStatusCode"] = toStatusCode
                });

        public static Error InvalidLineInitialStatus(string statusCode)
    => Error.Create(
        code: "PlanVersion.InvalidLineInitialStatus",
        message: "خط جدید فقط با وضعیت «برنامه‌ریزی‌شده» (planned) ساخته می‌شود.",
        details: new Dictionary<string, object>
        {
            ["InitialStatusCode"] = statusCode
        });

        public static Error InvalidSubmitTargetStatus(string statusCode)
            => Error.Create(
                code: "PlanVersion.InvalidSubmitTargetStatus",
                message: "وضعیت مقصد ارسال برای تأیید فقط می‌تواند «در انتظار تأیید» (pending_approval) باشد.",
                details: new Dictionary<string, object>
                {
                    ["TargetStatusCode"] = statusCode
                });

        public static Error InvalidWithdrawTargetStatus(string statusCode)
            => Error.Create(
                code: "PlanVersion.InvalidWithdrawTargetStatus",
                message: "وضعیت مقصد بازگشت فقط می‌تواند «پیش‌نویس» (draft) باشد.",
                details: new Dictionary<string, object>
                {
                    ["TargetStatusCode"] = statusCode
                });

        public static Error InvalidDecisionTargetStatuses(string? approvedStatusCode, string? rejectedStatusCode)
            => Error.Create(
                code: "PlanVersion.InvalidDecisionTargetStatuses",
                message: "کد وضعیت تأیید باید «approved» و کد وضعیت رد باید «rejected» باشد.",
                details: new Dictionary<string, object>
                {
                    ["ApprovedStatusCode"] = approvedStatusCode ?? string.Empty,
                    ["RejectedStatusCode"] = rejectedStatusCode ?? string.Empty
                });

        public static Error InvalidSupersededTargetStatus(string statusCode)
            => Error.Create(
                code: "PlanVersion.InvalidSupersededTargetStatus",
                message: "وضعیت مقصد جایگزینی فقط می‌تواند «superseded» باشد.",
                details: new Dictionary<string, object>
                {
                    ["TargetStatusCode"] = statusCode
                });

        public static Error InvalidExecutionTargetStatus(string statusCode)
            => Error.Create(
                code: "PlanVersion.InvalidExecutionTargetStatus",
                message: "ثبت اجرا خط را با وضعیت «انجام‌شده» (done) می‌بندد؛ وضعیت مقصد دیگری مجاز نیست.",
                details: new Dictionary<string, object>
                {
                    ["TargetStatusCode"] = statusCode
                });

        public static Error WithdrawBlockedByDecisions()
            => Error.Create(
                code: "PlanVersion.WithdrawBlockedByDecisions",
                message: "برنامه‌ای که دست‌کم یک تصمیم تأیید دارد قابل بازگشت به پیش‌نویس نیست؛ منتظر تصمیم نقش دیگر بمانید یا نسخهٔ جدید بسازید.");
    }
}
