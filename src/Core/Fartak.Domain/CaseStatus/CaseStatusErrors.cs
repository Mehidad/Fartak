using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fartak.Domain.CaseStatus
{
    public static class CaseStatusErrors
    {
        public static Error NullReferenceError()
            => Error.Create(
                code: "CaseStatus.NullReference",
                message: "مقادیر اجباری وضعیت پرونده ارسال نشده است.");

        public static Error InvalidCode(string? code)
            => Error.Create(
                code: "CaseStatus.InvalidCode",
                message: "کد وضعیت پرونده نامعتبر است و نمی‌تواند خالی باشد.",
                details: new Dictionary<string, object>
                {
                    ["Code"] = code ?? string.Empty
                });

        public static Error CodeTooLong(string code)
            => Error.Create(
                code: "CaseStatus.CodeTooLong",
                message: "کد وضعیت پرونده نباید بیش از ۴۰ کاراکتر باشد.",
                details: new Dictionary<string, object>
                {
                    ["Code"] = code
                });

        public static Error InvalidTitle(string? title)
            => Error.Create(
                code: "CaseStatus.InvalidTitle",
                message: "عنوان وضعیت پرونده نامعتبر است و نمی‌تواند خالی باشد.",
                details: new Dictionary<string, object>
                {
                    ["Title"] = title ?? string.Empty
                });

        public static Error TitleTooLong(string title)
            => Error.Create(
                code: "CaseStatus.TitleTooLong",
                message: "عنوان وضعیت پرونده نباید بیش از ۱۲۰ کاراکتر باشد.",
                details: new Dictionary<string, object>
                {
                    ["Title"] = title
                });

        public static Error TitleUnchanged()
            => Error.Create(
                code: "CaseStatus.TitleUnchanged",
                message: "عنوان جدید همان عنوان فعلی وضعیت پرونده است.");

        public static Error InvalidGroupCode(string? groupCode)
            => Error.Create(
                code: "CaseStatus.InvalidGroupCode",
                message: "کد گروه چرخه پرونده نامعتبر است و نمی‌تواند خالی باشد.",
                details: new Dictionary<string, object>
                {
                    ["GroupCode"] = groupCode ?? string.Empty
                });

        public static Error GroupCodeTooLong(string groupCode)
            => Error.Create(
                code: "CaseStatus.GroupCodeTooLong",
                message: "کد گروه چرخه پرونده نباید بیش از ۳۰ کاراکتر باشد.",
                details: new Dictionary<string, object>
                {
                    ["GroupCode"] = groupCode
                });

        public static Error SortOrderOutOfRange(int sortOrder)
            => Error.Create(
                code: "CaseStatus.SortOrderOutOfRange",
                message: "ترتیب نمایش باید عددی بین ۰ تا ۳۲۷۶۷ باشد.",
                details: new Dictionary<string, object>
                {
                    ["SortOrder"] = sortOrder
                });

        public static Error SortOrderUnchanged()
            => Error.Create(
                code: "CaseStatus.SortOrderUnchanged",
                message: "ترتیب نمایش جدید همان ترتیب فعلی وضعیت پرونده است.");

        public static Error InvalidColor(string? color)
            => Error.Create(
                code: "CaseStatus.InvalidColor",
                message: "رنگ نمایش وضعیت نامعتبر است.",
                details: new Dictionary<string, object>
                {
                    ["Color"] = color ?? string.Empty
                });

        public static Error ColorTooLong(string color)
            => Error.Create(
                code: "CaseStatus.ColorTooLong",
                message: "رنگ نمایش وضعیت نباید بیش از ۱۰ کاراکتر باشد.",
                details: new Dictionary<string, object>
                {
                    ["Color"] = color
                });

        public static Error ColorUnchanged()
            => Error.Create(
                code: "CaseStatus.ColorUnchanged",
                message: "رنگ ارسالی همان رنگ فعلی وضعیت پرونده است.");

        public static Error TerminalityUnchanged()
            => Error.Create(
                code: "CaseStatus.TerminalityUnchanged",
                message: "وضعیت پایانی‌بودن این وضعیت قبلاً با همین مقدار تنظیم شده است.");

        public static Error AddTransitionFromTerminalStatus()
            => Error.Create(
                code: "CaseStatus.AddTransitionFromTerminalStatus",
                message: "از وضعیت پایانی گذار خروجی تعریف نمی‌شود.");

        public static Error TerminalStatusWithOutgoingTransitions()
            => Error.Create(
                code: "CaseStatus.TerminalStatusWithOutgoingTransitions",
                message: "این وضعیت گذار خروجی دارد و نمی‌تواند وضعیت پایانی اعلام شود.");

        public static Error TransitionNullReference()
            => Error.Create(
                code: "CaseStatus.TransitionNullReference",
                message: "شناسه انتقال یا وضعیت مقصد گذار ارسال نشده است.");

        public static Error SelfTransition()
            => Error.Create(
                code: "CaseStatus.SelfTransition",
                message: "گذار از یک وضعیت به خودش معنا ندارد.");

        public static Error TransitionAlreadyExists(long toStatusId)
            => Error.Create(
                code: "CaseStatus.TransitionAlreadyExists",
                message: "برای وضعیت مقصد قبلاً گذار خروجی تعریف شده است.",
                details: new Dictionary<string, object>
                {
                    ["ToStatusId"] = toStatusId
                });

        public static Error TransitionNotFound(long transitionId)
            => Error.Create(
                code: "CaseStatus.TransitionNotFound",
                message: "گذار موردنظر در این وضعیت یافت نشد.",
                details: new Dictionary<string, object>
                {
                    ["TransitionId"] = transitionId
                });

        public static Error TransitionRoleCodeInvalid()
            => Error.Create(
                code: "CaseStatus.TransitionRoleCodeInvalid",
                message: "فهرست نقش‌های مجاز نمی‌تواند شامل مقدار خالی باشد.");

        public static Error TransitionRoleCodeTooLong(string roleCode)
            => Error.Create(
                code: "CaseStatus.TransitionRoleCodeTooLong",
                message: "کد نقش نباید بیش از ۴۰ کاراکتر باشد.",
                details: new Dictionary<string, object>
                {
                    ["RoleCode"] = roleCode
                });

        public static Error TransitionRoleCodeDuplicated(string roleCode)
            => Error.Create(
                code: "CaseStatus.TransitionRoleCodeDuplicated",
                message: "کد نقش در فهرست نقش‌های مجاز تکراری است.",
                details: new Dictionary<string, object>
                {
                    ["RoleCode"] = roleCode
                });

        public static Error TransitionNoteTooLong(string note)
            => Error.Create(
                code: "CaseStatus.TransitionNoteTooLong",
                message: "یادداشت گذار نباید بیش از ۱۰۰۰ کاراکتر باشد.",
                details: new Dictionary<string, object>
                {
                    ["Note"] = note
                });

        public static Error TransitionRolesUnchanged()
            => Error.Create(
                code: "CaseStatus.TransitionRolesUnchanged",
                message: "فهرست نقش‌های مجاز ارسالی همان فهرست فعلی گذار است.");

        public static Error TransitionAutomaticityUnchanged()
            => Error.Create(
                code: "CaseStatus.TransitionAutomaticityUnchanged",
                message: "وضعیت خودکار/دستی گذار قبلاً با همین مقدار تنظیم شده است.");

        public static Error TransitionNoteUnchanged()
            => Error.Create(
                code: "CaseStatus.TransitionNoteUnchanged",
                message: "یادداشت ارسالی همان یادداشت فعلی گذار است.");
    }
}
