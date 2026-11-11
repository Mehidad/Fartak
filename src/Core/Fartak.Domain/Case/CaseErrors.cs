using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Case
{
    public static class CaseErrors
    {
        public static Error NullReferenceError()
            => Error.Create(
                code: "Case.NullReference",
                message: "مقادیر اجباری پرونده ارسال نشده است.");

        public static Error AlreadyInStatus(long caseId, long statusId)
            => Error.Create(
                code: "Case.AlreadyInStatus",
                message: "پرونده از قبل در همین وضعیت قرار دارد.",
                details: new Dictionary<string, object>
                {
                    ["CaseId"] = caseId,
                    ["StatusId"] = statusId
                });

        public static Error InvalidDeadline(DateTime deadline)
            => Error.Create(
                code: "Case.InvalidDeadline",
                message: "مهلت پاسخ کارشناس باید در آینده و بعد از زمان تخصیص باشد.",
                details: new Dictionary<string, object>
                {
                    ["Deadline"] = deadline.ToString("yyyy-MM-dd HH:mm:ss")
                });

        public static Error OpenAssignmentExists(long caseId)
            => Error.Create(
                code: "Case.OpenAssignmentExists",
                message: "پرونده یک تخصیص بازِ پاسخ‌داده‌نشده دارد؛ ابتدا نتیجه تخصیص فعلی را ثبت کنید.",
                details: new Dictionary<string, object>
                {
                    ["CaseId"] = caseId
                });

        public static Error NoOpenAssignment(long caseId)
            => Error.Create(
                code: "Case.NoOpenAssignment",
                message: "پرونده تخصیص بازِ در انتظار پاسخ ندارد.",
                details: new Dictionary<string, object>
                {
                    ["CaseId"] = caseId
                });

        public static Error AssignmentAlreadyResponded(long assignmentId)
            => Error.Create(
                code: "Case.AssignmentAlreadyResponded",
                message: "این تخصیص از قبل پاسخ داده شده است.",
                details: new Dictionary<string, object>
                {
                    ["AssignmentId"] = assignmentId
                });

        public static Error InvalidRespondedAt(DateTime respondedAt, DateTime assignedAt)
            => Error.Create(
                code: "Case.InvalidRespondedAt",
                message: "زمان پاسخ نمی‌تواند قبل از زمان تخصیص باشد.",
                details: new Dictionary<string, object>
                {
                    ["RespondedAt"] = respondedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    ["AssignedAt"] = assignedAt.ToString("yyyy-MM-dd HH:mm:ss")
                });

        public static Error NoAssignedExpert(long caseId)
            => Error.Create(
                code: "Case.NoAssignedExpert",
                message: "پرونده کارشناس تخصیص‌یافته‌ای ندارد و این عملیات امکان‌پذیر نیست.",
                details: new Dictionary<string, object>
                {
                    ["CaseId"] = caseId
                });

        public static Error InvalidNote()
            => Error.Create(
                code: "Case.InvalidNote",
                message: "متن یادداشت خالی است.");

        public static Error NoteTooLong(int length)
            => Error.Create(
                code: "Case.NoteTooLong",
                message: "متن یادداشت نباید بیش از ۴٬۰۰۰ کاراکتر باشد.",
                details: new Dictionary<string, object>
                {
                    ["Length"] = length
                });
    }
}
