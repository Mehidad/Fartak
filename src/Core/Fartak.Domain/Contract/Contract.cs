using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Case;
using Fartak.Domain.Common;
using Fartak.Domain.Contract.Events;
using Fartak.Domain.Framework;
using Fartak.Domain.User;

namespace Fartak.Domain.Contract
{
    // قرارداد (جدول contracts) — توافق مالی میان کشاورز و کارشناس/پلتفرم در چارچوب یک پرونده؛
    // طبق وایرفریم‌ها: عقد قرارداد پس از تأیید پرونده و پیش از پرداخت‌های مرحله‌ای.
    //
    // مبلغ‌ها با bigint ریال ذخیره می‌شوند (همان قاعدهٔ plan_lines / total_estimated_cost_rial)
    // و درصد کارمزد از VO مشترک CommissionRate (Fartak.Domain.Case) بازمصرف می‌شود؛
    // مقدار تهی یعنی «کارمزد پیش‌فرض تنظیمات سیستم» (همان معنای commission_rate تهی در پرونده).
    //
    // وضعیت با lookup: contract_status مدیریت می‌شود (پیش‌نویس/فعال/تکمیل/لغو/فسخ)؛
    // تجمع فقط LookupId را نگه می‌دارد و اعتبارسنجی گذارها در لایه Application با کد وضعیت
    // انجام می‌شود — همان الگوی VisitSchedule. ضمانت‌نامه بانکی اختیاری است و
    // با SetBankGuarantee/ClearBankGuarantee به‌صورت یکجا ثبت/پاک می‌شود (Snapshot چهار ستونی).
    //
    // تغییرناپذیرها پس از ایجاد: CaseId و FarmerId؛ قرارداد برای پروندهٔ دیگر یعنی رکورد جدید.
    // طبق قاعده timestamp/is_active: چون قرارداد سند قابل ویرایش است UpdatedAt نگه می‌شود؛
    // is_active نداریم → متد Activate/Deactivate ندارد؛ چرخهٔ حیات با status_id است.
    public sealed class Contract : AggregateRoot<ContractId>
    {
        public const int MaxTitleLength = 200;
        public const int MaxBankNameLength = 100;
        public const int MaxGuaranteeNumberLength = 50;
        public const int MaxNoteLength = 500;

        public CaseId CaseId { get; private set; }
        public UserId FarmerId { get; private set; }                // طرف قرارداد: کشاورز
        public UserId? ExpertId { get; private set; }               // کارشناس مجری (اختیاری)
        public string Title { get; private set; }                   // عنوان قرارداد مثل «تولید گندم سفید - فصل ۱۴۰۳»
        public long TotalAmountRial { get; private set; }           // مبلغ کل قرارداد (ریال) — >= 0
        public CommissionRate? CommissionRate { get; private set; } // کارمزد اختصاصی (null = پیش‌فرض سیستم)
        public LookupId StatusId { get; private set; }              // lookup: contract_status
        public DateOnly? StartDate { get; private set; }            // از تاریخ
        public DateOnly? EndDate { get; private set; }              // تا تاریخ
        public string? BankName { get; private set; }               // ضمانت‌نامه بانکی: نام بانک
        public string? GuaranteeNumber { get; private set; }        // شماره ضمانت‌نامه
        public long? GuaranteeAmountRial { get; private set; }      // مبلغ ضمانت‌نامه (ریال)
        public DateOnly? GuaranteeValidUntil { get; private set; }  // اعتبار ضمانت‌نامه
        public string? Note { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        private Contract() { } // برای EF Core

        private Contract(
            ContractId id,
            CaseId caseId,
            UserId farmerId,
            UserId? expertId,
            string title,
            long totalAmountRial,
            CommissionRate? commissionRate,
            LookupId statusId,
            DateOnly? startDate,
            DateOnly? endDate)
            : base(id)
        {
            CaseId = caseId;
            FarmerId = farmerId;
            ExpertId = expertId;
            Title = title;
            TotalAmountRial = totalAmountRial;
            CommissionRate = commissionRate;
            StatusId = statusId;
            StartDate = startDate;
            EndDate = endDate;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = CreatedAt;
        }

        public static Result<Contract> Create(
            ContractId id,
            CaseId caseId,
            UserId farmerId,
            UserId? expertId,
            string title,
            long totalAmountRial,
            CommissionRate? commissionRate,
            LookupId statusId,
            DateOnly? startDate,
            DateOnly? endDate)
        {
            if (id is null)
                return Result.Failure<Contract>(ContractErrors.NullReference);

            if (caseId == CaseId.Empty)
                return Result.Failure<Contract>(ContractErrors.CaseNotSpecified);

            if (farmerId == UserId.Empty)
                return Result.Failure<Contract>(ContractErrors.FarmerNotSpecified);

            if (statusId == LookupId.Empty)
                return Result.Failure<Contract>(ContractErrors.StatusNotSpecified);

            var titleValidation = ValidateTitle(title);
            if (titleValidation.IsFailure)
                return Result.Failure<Contract>(titleValidation.Error);

            var amountValidation = ValidateAmount(totalAmountRial);
            if (amountValidation.IsFailure)
                return Result.Failure<Contract>(amountValidation.Error);

            if (IsInvalidDateRange(startDate, endDate))
                return Result.Failure<Contract>(ContractErrors.InvalidDateRange);

            var contract = new Contract(
                id, caseId, farmerId, expertId,
                title.Trim(), totalAmountRial, commissionRate, statusId,
                startDate, endDate);

            contract.AddDomainEvent(new ContractCreatedEvent(
                id.Value,
                caseId.Value,
                farmerId.Value,
                expertId?.Value,
                contract.Title,
                totalAmountRial,
                commissionRate?.Rate,
                statusId.Value,
                startDate,
                endDate));

            return Result.Success(contract);
        }

        // گذار وضعیت قرارداد — اعتبارسنجی ماشین وضعیت با کد lookup در لایه Application
        public Result ChangeStatus(LookupId newStatusId)
        {
            if (newStatusId == LookupId.Empty)
                return Result.Failure(ContractErrors.StatusNotSpecified);

            if (newStatusId == StatusId)
                return Result.Failure(ContractErrors.SameStatus);

            var oldStatusId = StatusId;
            StatusId = newStatusId;
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new ContractStatusChangedEvent(
                Id.Value,
                CaseId.Value,
                oldStatusId.Value,
                newStatusId.Value));

            return Result.Success();
        }

        // ویرایش شرایط قرارداد (عنوان، مبلغ کل، کارمزد، بازهٔ اجرا) — کارمزد تهی یعنی پیش‌فرض سیستم
        public Result UpdateTerms(
            string title,
            long totalAmountRial,
            CommissionRate? commissionRate,
            DateOnly? startDate,
            DateOnly? endDate)
        {
            var titleValidation = ValidateTitle(title);
            if (titleValidation.IsFailure)
                return titleValidation;

            var amountValidation = ValidateAmount(totalAmountRial);
            if (amountValidation.IsFailure)
                return amountValidation;

            if (IsInvalidDateRange(startDate, endDate))
                return Result.Failure(ContractErrors.InvalidDateRange);

            Title = title.Trim();
            TotalAmountRial = totalAmountRial;
            CommissionRate = commissionRate;
            StartDate = startDate;
            EndDate = endDate;
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new ContractTermsUpdatedEvent(
                Id.Value,
                CaseId.Value,
                Title,
                TotalAmountRial,
                commissionRate?.Rate,
                startDate,
                endDate));

            return Result.Success();
        }

        // ثبت ضمانت‌نامه بانکی به‌صورت یکجا — فقط وقتی قبلاً ثبت نشده باشد
        public Result SetBankGuarantee(string bankName, string guaranteeNumber, long amountRial, DateOnly validUntil)
        {
            if (BankName is not null)
                return Result.Failure(ContractErrors.BankGuaranteeAlreadySet);

            if (string.IsNullOrWhiteSpace(bankName))
                return Result.Failure(ContractErrors.BankNameNotSpecified);

            if (bankName.Trim().Length > MaxBankNameLength)
                return Result.Failure(ContractErrors.BankNameTooLong(bankName.Trim().Length));

            if (string.IsNullOrWhiteSpace(guaranteeNumber))
                return Result.Failure(ContractErrors.GuaranteeNumberNotSpecified);

            if (guaranteeNumber.Trim().Length > MaxGuaranteeNumberLength)
                return Result.Failure(ContractErrors.GuaranteeNumberTooLong(guaranteeNumber.Trim().Length));

            if (amountRial <= 0)
                return Result.Failure(ContractErrors.InvalidGuaranteeAmount);

            BankName = bankName.Trim();
            GuaranteeNumber = guaranteeNumber.Trim();
            GuaranteeAmountRial = amountRial;
            GuaranteeValidUntil = validUntil;
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new ContractBankGuaranteeSetEvent(
                Id.Value,
                CaseId.Value,
                BankName,
                GuaranteeNumber,
                amountRial,
                validUntil));

            return Result.Success();
        }

        // پاک‌کردن ضمانت‌نامه بانکی — فقط وقتی قبلاً ثبت شده باشد
        public Result ClearBankGuarantee()
        {
            if (BankName is null)
                return Result.Failure(ContractErrors.BankGuaranteeNotSet);

            BankName = null;
            GuaranteeNumber = null;
            GuaranteeAmountRial = null;
            GuaranteeValidUntil = null;
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new ContractBankGuaranteeClearedEvent(
                Id.Value,
                CaseId.Value));

            return Result.Success();
        }

        public Result SetNote(string? note)
        {
            var trimmedNote = note?.Trim();
            if (trimmedNote is not null && trimmedNote.Length > MaxNoteLength)
                return Result.Failure(ContractErrors.NoteTooLong);

            var oldNote = Note;
            Note = trimmedNote;
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new ContractNoteChangedEvent(
                Id.Value,
                CaseId.Value,
                oldNote,
                trimmedNote));

            return Result.Success();
        }

        private static Result ValidateTitle(string? title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return Result.Failure(ContractErrors.TitleNotSpecified);

            if (title.Trim().Length > MaxTitleLength)
                return Result.Failure(ContractErrors.TitleTooLong(title.Trim().Length));

            return Result.Success();
        }

        private static Result ValidateAmount(long totalAmountRial)
        {
            if (totalAmountRial < 0)
                return Result.Failure(ContractErrors.InvalidTotalAmount);

            return Result.Success();
        }

        private static bool IsInvalidDateRange(DateOnly? startDate, DateOnly? endDate)
            => startDate is not null && endDate is not null && endDate.Value < startDate.Value;
    }
}
