using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Case;
using Fartak.Domain.Framework;

namespace Fartak.Domain.CaseStatus
{
    // قاعده گذار وضعیت (جدول case_status_transitions) — موجودیت فرزندِ CaseStatus؛
    // گذارهای خروجیِ همین وضعیت را نگه می‌دارد و فقط از طریق خود Aggregate ساخته/ویرایش می‌شود.
    // from_status_id ستون صریح ندارد (مطابق الگوی CaseNote) و با navigation در EF نگاشت می‌شود.
    public sealed class CaseStatusTransition : Entity<CaseStatusTransitionId>
    {
        // سقف عملیاتی یادداشت توضیحی گذار (ستون note از نوع text است و در DB سقف ندارد)
        public const int MaxNoteLength = 1000;

        // سقف طول کد نقش مطابق roles.code (varchar 40 در ERD)
        public const int MaxRoleCodeLength = 40;

        public CaseStatusId ToStatusId { get; private set; }        // وضعیت مقصد گذار
        public IReadOnlyList<string> AllowedRoleCodes { get; private set; }  // خالی = فقط سیستم
        public bool IsAutomatic { get; private set; }               // گذار توسط سیستم به‌صورت خودکار انجام می‌شود
        public string? Note { get; private set; }                   // یادداشت توضیحی قاعده گذار

        private CaseStatusTransition() { } // برای EF Core

        private CaseStatusTransition(
            CaseStatusTransitionId id,
            CaseStatusId toStatusId,
            List<string> allowedRoleCodes,
            bool isAutomatic,
            string? note)
        {
            Id = id;
            ToStatusId = toStatusId;
            AllowedRoleCodes = allowedRoleCodes;
            IsAutomatic = isAutomatic;
            Note = note;
        }

        // فقط ریشه تجمع صدا می‌زند؛ اعتبارسنجی فهرست نقش‌ها و یادداشت پیش‌تر در ریشه انجام شده است
        internal static CaseStatusTransition Create(
            CaseStatusTransitionId id,
            CaseStatusId toStatusId,
            List<string> allowedRoleCodes,
            bool isAutomatic,
            string? note)
        {
            if (id is null || toStatusId is null)
                throw new DomainValidationException(
                    "CaseStatus.TransitionInvalid",
                    "شناسه انتقال یا وضعیت مقصد گذار ارسال نشده است.");

            return new CaseStatusTransition(id, toStatusId, allowedRoleCodes, isAutomatic, note);
        }

        internal void SetRoles(List<string> allowedRoleCodes)
            => AllowedRoleCodes = allowedRoleCodes;

        internal void SetAutomaticity(bool isAutomatic)
            => IsAutomatic = isAutomatic;

        internal void SetNote(string? note)
            => Note = note;
    }
}
