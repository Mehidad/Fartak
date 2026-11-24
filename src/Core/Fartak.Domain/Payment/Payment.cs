using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Case;
using Fartak.Domain.Common;
using Fartak.Domain.Contract;
using Fartak.Domain.Framework;
using Fartak.Domain.Payment.Events;
using Fartak.Domain.User;

namespace Fartak.Domain.Payment
{
    // پرداخت (جدول payments) — مبالغ پرداختی کشاورز/پلتفرم در چارچوب پرونده؛
    // طبق وایرفریم‌ها: پرداخت مرحله‌ای، علی‌الحساب و تسویهٔ قرارداد.
    //
    // kind/method/status همه با lookup مدیریت می‌شوند (payment_kind / payment_method / payment_status)؛
    // تجمع فقط LookupId نگه می‌دارد و اعتبارسنجی گذارها در لایه Application با کد وضعیت انجام می‌شود
    // — همان الگوی VisitSchedule. گارد وضعیتیِ وابسته به کد lookup اینجا نداریم؛ به‌جای آن
    // «PaidAt» نقش نگهبان را بازی می‌کند: پس از ثبت قطعی پرداخت، دوباره «پرداخت‌شده» یا «لغو»
    // ممکن نیست (AlreadyPaid / CannotCancelPaidPayment) — تاریخ‌ها متعلق به دامنه‌اند و
    // به seed lookup وابسته نیستند.
    //
    // مبلغ با bigint ریال ذخیره می‌شود (همان قاعدهٔ plan_lines و contracts).
    // تغییرناپذیرها پس از ایجاد: CaseId و AmountRial؛ مبلغ متفاوت یعنی رکورد جدید.
    // طبق قاعده timestamp/is_active: UpdatedAt نگه می‌شود؛ is_active نداریم →
    // Activate/Deactivate ندارد؛ چرخهٔ حیات با status_id است.
    public sealed class Payment : AggregateRoot<PaymentId>
    {
        public const int MaxReferenceNoLength = 50;
        public const int MaxNoteLength = 500;

        public CaseId CaseId { get; private set; }
        public ContractId? ContractId { get; private set; }   // قرارداد مرتبط (اختیاری)
        public UserId? PayerId { get; private set; }          // پرداخت‌کننده (NULL = پلتفرم/سیستم)
        public UserId? PayeeId { get; private set; }          // دریافت‌کننده (اختیاری)
        public long AmountRial { get; private set; }          // مبلغ پرداخت (ریال) — > 0
        public LookupId KindId { get; private set; }          // lookup: payment_kind
        public LookupId? MethodId { get; private set; }       // lookup: payment_method
        public LookupId StatusId { get; private set; }        // lookup: payment_status
        public string? ReferenceNo { get; private set; }      // شماره پیگیری/رسید
        public DateTime? PaidAt { get; private set; }         // زمان ثبت قطعی پرداخت
        public string? Note { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        private Payment() { } // برای EF Core

        private Payment(
            PaymentId id,
            CaseId caseId,
            ContractId? contractId,
            UserId? payerId,
            UserId? payeeId,
            long amountRial,
            LookupId kindId,
            LookupId? methodId,
            LookupId statusId,
            string? referenceNo,
            string? note)
            : base(id)
        {
            CaseId = caseId;
            ContractId = contractId;
            PayerId = payerId;
            PayeeId = payeeId;
            AmountRial = amountRial;
            KindId = kindId;
            MethodId = methodId;
            StatusId = statusId;
            ReferenceNo = referenceNo;
            Note = note;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = CreatedAt;
        }

        public static Result<Payment> Create(
            PaymentId id,
            CaseId caseId,
            ContractId? contractId,
            UserId? payerId,
            UserId? payeeId,
            long amountRial,
            LookupId kindId,
            LookupId? methodId,
            LookupId statusId,
            string? referenceNo,
            string? note)
        {
            if (id is null)
                return Result.Failure<Payment>(PaymentErrors.NullReference);

            if (caseId == CaseId.Empty)
                return Result.Failure<Payment>(PaymentErrors.CaseNotSpecified);

            if (amountRial <= 0)
                return Result.Failure<Payment>(PaymentErrors.InvalidAmount);

            if (kindId == LookupId.Empty)
                return Result.Failure<Payment>(PaymentErrors.KindNotSpecified);

            if (statusId == LookupId.Empty)
                return Result.Failure<Payment>(PaymentErrors.StatusNotSpecified);

            var trimmedReferenceNo = referenceNo?.Trim();
            if (trimmedReferenceNo is not null && trimmedReferenceNo.Length > MaxReferenceNoLength)
                return Result.Failure<Payment>(PaymentErrors.ReferenceNoTooLong(trimmedReferenceNo.Length));

            var trimmedNote = note?.Trim();
            if (trimmedNote is not null && trimmedNote.Length > MaxNoteLength)
                return Result.Failure<Payment>(PaymentErrors.NoteTooLong);

            var payment = new Payment(
                id, caseId, contractId, payerId, payeeId,
                amountRial, kindId, methodId, statusId,
                trimmedReferenceNo, trimmedNote);

            payment.AddDomainEvent(new PaymentCreatedEvent(
                id.Value,
                caseId.Value,
                contractId?.Value,
                payerId?.Value,
                payeeId?.Value,
                amountRial,
                kindId.Value,
                methodId?.Value,
                statusId.Value,
                trimmedReferenceNo,
                trimmedNote));

            return Result.Success(payment);
        }

        // گذار وضعیت پرداخت — اعتبارسنجی ماشین وضعیت با کد lookup در لایه Application
        public Result ChangeStatus(LookupId newStatusId)
        {
            if (newStatusId == LookupId.Empty)
                return Result.Failure(PaymentErrors.StatusNotSpecified);

            if (newStatusId == StatusId)
                return Result.Failure(PaymentErrors.SameStatus);

            var oldStatusId = StatusId;
            StatusId = newStatusId;
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new PaymentStatusChangedEvent(
                Id.Value,
                CaseId.Value,
                oldStatusId.Value,
                newStatusId.Value));

            return Result.Success();
        }

        // ثبت قطعی پرداخت: وضعیت + زمان پرداخت + شماره پیگیری به‌صورت یکجا؛
        // پس از این فراخوانی، لغو یا ثبت مجدد ممکن نیست
        public Result MarkPaid(LookupId paidStatusId, string? referenceNo, DateTime paidAtUtc)
        {
            if (paidStatusId == LookupId.Empty)
                return Result.Failure(PaymentErrors.StatusNotSpecified);

            if (PaidAt is not null)
                return Result.Failure(PaymentErrors.AlreadyPaid);

            if (paidAtUtc > DateTime.UtcNow)
                return Result.Failure(PaymentErrors.PaidAtInFuture);

            var trimmedReferenceNo = referenceNo?.Trim();
            if (trimmedReferenceNo is not null && trimmedReferenceNo.Length > MaxReferenceNoLength)
                return Result.Failure(PaymentErrors.ReferenceNoTooLong(trimmedReferenceNo.Length));

            StatusId = paidStatusId;
            ReferenceNo = trimmedReferenceNo;
            PaidAt = paidAtUtc;
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new PaymentMarkedPaidEvent(
                Id.Value,
                CaseId.Value,
                AmountRial,
                trimmedReferenceNo,
                PaidAt.Value));

            return Result.Success();
        }

        // لغو پرداختِ ثبت‌نشده — دلیل لغو در یادداشت ذخیره می‌شود
        public Result Cancel(LookupId canceledStatusId, string? reason)
        {
            if (canceledStatusId == LookupId.Empty)
                return Result.Failure(PaymentErrors.StatusNotSpecified);

            if (PaidAt is not null)
                return Result.Failure(PaymentErrors.CannotCancelPaidPayment);

            var canceledAt = DateTime.UtcNow;
            StatusId = canceledStatusId;
            if (!string.IsNullOrWhiteSpace(reason))
                Note = reason.Trim();
            UpdatedAt = canceledAt;

            AddDomainEvent(new PaymentCanceledEvent(
                Id.Value,
                CaseId.Value,
                AmountRial,
                reason?.Trim(),
                canceledAt));

            return Result.Success();
        }

        public Result SetNote(string? note)
        {
            var trimmedNote = note?.Trim();
            if (trimmedNote is not null && trimmedNote.Length > MaxNoteLength)
                return Result.Failure(PaymentErrors.NoteTooLong);

            var oldNote = Note;
            Note = trimmedNote;
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new PaymentNoteChangedEvent(
                Id.Value,
                CaseId.Value,
                oldNote,
                trimmedNote));

            return Result.Success();
        }
    }
}
