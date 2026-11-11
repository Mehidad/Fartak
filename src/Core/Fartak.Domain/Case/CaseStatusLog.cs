using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;
using Fartak.Domain.User.Fartak.Domain.User;

namespace Fartak.Domain.Case
{
    // یک رکورد از تاریخچه تغییر وضعیت — موجودیت فرزندِ Case (جدول case_status_logs)
    // فقط از طریق خود Aggregate ساخته می‌شود: Case.Create و Case.ChangeStatus
    // FromStatusId تهی یعنی «وضعیت نخستِ پرونده هنگام ایجاد».
    public sealed class CaseStatusLog : Entity<CaseStatusLogId>
    {
        public CaseStatusId? FromStatusId { get; private set; }
        public CaseStatusId ToStatusId { get; private set; }
        public UserId? ChangedBy { get; private set; }       // NULL = تغییر خودکار سیستم
        public DateTime ChangedAt { get; private set; }
        public bool IsAutomatic { get; private set; }
        public string? Note { get; private set; }

        private CaseStatusLog() { } // برای EF Core

        private CaseStatusLog(
            CaseStatusLogId id,
            CaseStatusId? fromStatusId,
            CaseStatusId toStatusId,
            UserId? changedBy,
            bool isAutomatic,
            string? note)
        {
            Id = id;
            FromStatusId = fromStatusId;
            ToStatusId = toStatusId;
            ChangedBy = changedBy;
            IsAutomatic = isAutomatic;
            Note = note;
            ChangedAt = DateTime.UtcNow;
        }

        internal static CaseStatusLog Create(
            CaseStatusLogId id,
            CaseStatusId? fromStatusId,
            CaseStatusId toStatusId,
            UserId? changedBy,
            bool isAutomatic,
            string? note)
        {
            if (toStatusId is null)
                throw new DomainValidationException(
                    "Case.StatusNull",
                    "وضعیت مقصد برای ثبت در تاریخچه پرونده ارسال نشده است.");

            return new CaseStatusLog(id, fromStatusId, toStatusId, changedBy, isAutomatic, note);
        }
    }
}
