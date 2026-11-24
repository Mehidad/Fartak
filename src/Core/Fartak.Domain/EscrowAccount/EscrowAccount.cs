using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Case;
using Fartak.Domain.Common;
using Fartak.Domain.EscrowAccount.Events;
using Fartak.Domain.Framework;
using Fartak.Domain.Payment;
using Fartak.Domain.User;

namespace Fartak.Domain.EscrowAccount
{
    // حساب امانی/پانزده‌درصدی (جدول escrow_accounts) — طبق وایرفریم‌های فروش، مبلغ قرارداد
    // پس از پرداخت در حساب امانی نگه داشته می‌شود؛ کارمزد پلتفرم (پیش‌فرض ۱۵٪ طبق موکاپ)
    // کسر و باقی به دریافت‌کننده آزاد می‌شود.
    //
    // CommissionRate از VO مشترک Fartak.Domain.Case بازمصرف می‌شود؛ مقادیر کارمزد و سهم پرداختی
    // در لحظهٔ Create محاسبه و Snapshot می‌شوند تا تغییر نرخ بعدی، رکوردهای قبلی را تغییر ندهد.
    //
    // گاردهای وضعیت بدون وابستگی به کد lookup: ReleasedAt/RefundedAt متعلق به دامنه‌اند؛
    // پس از Release یا Refund، عمل دوم ممکن نیست (AlreadyReleased/AlreadyRefunded).
    // چرخهٔ حیات با lookup: escrow_status اعتبارسنجی می‌شود — همان الگوی VisitSchedule.
    //
    // طبق قاعده timestamp/is_active: تاریخ‌های چرخهٔ حیات خودِ فیلدهای دامنه‌اند
    // (OpenedAt/ReleasedAt/RefundedAt)؛ is_active نداریم → Activate/Deactivate ندارد.
    public sealed class EscrowAccount : AggregateRoot<EscrowAccountId>
    {
        public CaseId CaseId { get; private set; }
        public PaymentId? PaymentId { get; private set; }        // منبع وجه (پرداخت مرتبط؛ اختیاری)
        public UserId FarmerId { get; private set; }              // صاحب حساب امانی
        public UserId? ExpertId { get; private set; }             // دریافت‌کنندهٔ نهایی (اختیاری)
        public long HeldAmountRial { get; private set; }          // مبلغ نگه‌داشته‌شده (ریال) — > 0
        public CommissionRate CommissionRate { get; private set; } // درصد کارمزد پلتفرم
        public long CommissionAmountRial { get; private set; }    // سهم پلتفرم (Snapshot)
        public long PayoutAmountRial { get; private set; }        // سهم دریافت‌کننده (Snapshot)
        public LookupId StatusId { get; private set; }            // lookup: escrow_status
        public DateTime OpenedAt { get; private set; }
        public DateTime? ReleasedAt { get; private set; }
        public DateTime? RefundedAt { get; private set; }

        private EscrowAccount() { } // برای EF Core

        private EscrowAccount(
            EscrowAccountId id,
            CaseId caseId,
            PaymentId? paymentId,
            UserId farmerId,
            UserId? expertId,
            long heldAmountRial,
            CommissionRate commissionRate,
            long commissionAmountRial,
            long payoutAmountRial,
            LookupId statusId)
            : base(id)
        {
            CaseId = caseId;
            PaymentId = paymentId;
            FarmerId = farmerId;
            ExpertId = expertId;
            HeldAmountRial = heldAmountRial;
            CommissionRate = commissionRate;
            CommissionAmountRial = commissionAmountRial;
            PayoutAmountRial = payoutAmountRial;
            StatusId = statusId;
            OpenedAt = DateTime.UtcNow;
        }

        public static Result<EscrowAccount> Create(
            EscrowAccountId id,
            CaseId caseId,
            PaymentId? paymentId,
            UserId farmerId,
            UserId? expertId,
            long heldAmountRial,
            CommissionRate commissionRate,
            LookupId statusId)
        {
            if (id is null)
                return Result.Failure<EscrowAccount>(EscrowAccountErrors.NullReference);

            if (caseId == CaseId.Empty)
                return Result.Failure<EscrowAccount>(EscrowAccountErrors.CaseNotSpecified);

            if (farmerId == UserId.Empty)
                return Result.Failure<EscrowAccount>(EscrowAccountErrors.FarmerNotSpecified);

            if (heldAmountRial <= 0)
                return Result.Failure<EscrowAccount>(EscrowAccountErrors.InvalidHeldAmount);

            if (commissionRate is null)
                return Result.Failure<EscrowAccount>(EscrowAccountErrors.CommissionRateNotSpecified);

            if (statusId == LookupId.Empty)
                return Result.Failure<EscrowAccount>(EscrowAccountErrors.StatusNotSpecified);

            // تسویهٔ کارمزد در همین لحظه Snapshot می‌شود؛ تغییر نرخ آینده اثری روی این رکورد ندارد
            var commissionAmountRial = (long)Math.Round(
                heldAmountRial * commissionRate.Rate / 100m,
                0,
                MidpointRounding.AwayFromZero);

            var payoutAmountRial = heldAmountRial - commissionAmountRial;

            if (commissionAmountRial < 0 || payoutAmountRial < 0)
                return Result.Failure<EscrowAccount>(EscrowAccountErrors.InvalidCommissionRate);

            var escrow = new EscrowAccount(
                id, caseId, paymentId, farmerId, expertId,
                heldAmountRial, commissionRate,
                commissionAmountRial, payoutAmountRial, statusId);

            escrow.AddDomainEvent(new EscrowAccountOpenedEvent(
                id.Value,
                caseId.Value,
                paymentId?.Value,
                farmerId.Value,
                expertId?.Value,
                heldAmountRial,
                commissionRate.Rate,
                commissionAmountRial,
                payoutAmountRial,
                statusId.Value,
                escrow.OpenedAt));

            return Result.Success(escrow);
        }

        // آزادسازی وجه به دریافت‌کننده پس از کسر کارمزد — فقط یک‌بار ممکن است
        public Result Release(LookupId releasedStatusId)
        {
            if (ReleasedAt is not null)
                return Result.Failure(EscrowAccountErrors.AlreadyReleased);

            if (RefundedAt is not null)
                return Result.Failure(EscrowAccountErrors.AlreadyRefunded);

            if (releasedStatusId == LookupId.Empty)
                return Result.Failure(EscrowAccountErrors.StatusNotSpecified);

            StatusId = releasedStatusId;
            ReleasedAt = DateTime.UtcNow;

            AddDomainEvent(new EscrowFundsReleasedEvent(
                Id.Value,
                CaseId.Value,
                FarmerId.Value,
                ExpertId?.Value,
                PayoutAmountRial,
                CommissionAmountRial,
                ReleasedAt.Value));

            return Result.Success();
        }

        // برگرداندن وجه به کشاورز (انصراف/فسخ قرارداد) — فقط یک‌بار ممکن است
        public Result Refund(LookupId refundedStatusId)
        {
            if (RefundedAt is not null)
                return Result.Failure(EscrowAccountErrors.AlreadyRefunded);

            if (ReleasedAt is not null)
                return Result.Failure(EscrowAccountErrors.AlreadyReleased);

            if (refundedStatusId == LookupId.Empty)
                return Result.Failure(EscrowAccountErrors.StatusNotSpecified);

            StatusId = refundedStatusId;
            RefundedAt = DateTime.UtcNow;

            AddDomainEvent(new EscrowFundsRefundedEvent(
                Id.Value,
                CaseId.Value,
                FarmerId.Value,
                HeldAmountRial,
                RefundedAt.Value));

            return Result.Success();
        }
    }
}
