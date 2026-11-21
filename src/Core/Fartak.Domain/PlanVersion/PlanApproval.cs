using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Common;
using Fartak.Domain.Framework;
using Fartak.Domain.User.Fartak.Domain.User;

namespace Fartak.Domain.PlanVersion
{
    // ردیف تأیید دوگانه برنامه (جدول plan_approvals) — به‌ازای هر نسخه دو ردیف:
    // یک بار با نقش expert و یک بار با نقش farmer (UNIQUE نسخه+نقش در اسکیما).
    // ردیف هنگام SubmitForApproval به‌صورت «در انتظار» ساخته می‌شود و بعداً با تصمیم
    // پر می‌شود (approved_by / decided_at / decision_id / comment اختیاری در اسکیما).
    // کد نقش تأییدکننده شناسه معنایی است (CHECK اسکیما: expert/farmer) و تغییر نمی‌کند.
    public class PlanApproval : Entity<PlanApprovalId>
    {
        public string ApproverRole { get; private set; }   // expert | farmer (WellKnownApproverRoles)
        public UserId? ApprovedBy { get; private set; }
        public DateTime? DecidedAt { get; private set; }
        public LookupId? DecisionId { get; private set; }  // lookup: decision (approved/rejected)
        public string? Comment { get; private set; }

        // آیا تصمیم این نقش ثبت شده است؟ (مبنای گذار خودکار و ممنوعیت Withdraw)
        public bool IsDecided => DecidedAt is not null;

        private PlanApproval() { } // برای EF Core

        internal PlanApproval(PlanApprovalId id, string approverRole)
        {
            Id = id;
            ApproverRole = approverRole;
        }

        // ساخت ردیف در انتظار تأیید — فقط از طریق PlanVersion.SubmitForApproval
        internal static PlanApproval Create(PlanApprovalId id, string approverRole)
            => new PlanApproval(id, approverRole);

        internal void RecordDecision(LookupId decisionId, UserId? approvedBy, DateTime decidedAt, string? comment)
        {
            DecisionId = decisionId;
            ApprovedBy = approvedBy;
            DecidedAt = decidedAt;
            Comment = comment;
        }
    }
}
