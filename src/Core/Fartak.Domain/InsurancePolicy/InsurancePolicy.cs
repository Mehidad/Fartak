using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Case;
using Fartak.Domain.Common;
using Fartak.Domain.Framework;
using Fartak.Domain.InsurancePolicy.Events;

namespace Fartak.Domain.InsurancePolicy
{
    // بیمه‌نامهٔ محصول (جدول insurance_policies) — پوشش بیمه‌ای هر پرونده؛
    // در فاز بعد به API شرکت بیمه وصل می‌شود (اسکیما v0.9).
    //
    // چرخهٔ حیات با lookup مدیریت می‌شود (domain=insurance_status) — الگوی Payment؛
    // اعتبارسنجی گذارها با کد وضعیت در لایه Application انجام می‌شود.
    //
    // طبق قاعده timestamp/is_active: UpdatedAt نگه می‌شود (created_at دارد)؛
    // is_active نداریم → Activate/Deactivate ندارد.
    // meta (jsonb آزاد) — اعتبارسنجی ساختار در لایه Application انجام می‌شود.
    public sealed class InsurancePolicy : AggregateRoot<InsurancePolicyId>
    {
        public const int MaxCompanyNameLength = 120;    // varchar(120) NOT NULL
        public const int MaxPolicyNoLength = 60;        // varchar(60) NOT NULL

        public CaseId CaseId { get; private set; }                 // پروندهٔ بیمه‌شده
        public string CompanyName { get; private set; }            // نام شرکت بیمه
        public string PolicyNo { get; private set; }               // شماره بیمه‌نامه
        public DateTime? CoverageStart { get; private set; }       // شروع پوشش (date)
        public DateTime? CoverageEnd { get; private set; }         // پایان پوشش (date)
        public long? PremiumRial { get; private set; }             // حق بیمه (ریال) — >= 0
        public LookupId StatusId { get; private set; }             // lookup: insurance_status
        public string? Meta { get; private set; }                  // jsonb آزاد (اعتبارسنجی ساختار در لایه Application)
        public DateTime CreatedAt { get; private set; }

        private InsurancePolicy() { } // برای EF Core

        private InsurancePolicy(
            InsurancePolicyId id,
            CaseId caseId,
            string companyName,
            string policyNo,
            DateTime? coverageStart,
            DateTime? coverageEnd,
            long? premiumRial,
            LookupId statusId,
            string? meta)
            : base(id)
        {
            CaseId = caseId;
            CompanyName = companyName;
            PolicyNo = policyNo;
            CoverageStart = coverageStart;
            CoverageEnd = coverageEnd;
            PremiumRial = premiumRial;
            StatusId = statusId;
            Meta = meta;
            CreatedAt = DateTime.UtcNow;
        }

        public static Result<InsurancePolicy> Create(
            InsurancePolicyId id,
            CaseId caseId,
            string companyName,
            string policyNo,
            LookupId statusId,
            DateTime? coverageStartUtc = null,
            DateTime? coverageEndUtc = null,
            long? premiumRial = null,
            string? meta = null)
        {
            if (id is null)
                return Result.Failure<InsurancePolicy>(InsurancePolicyErrors.NullReference);

            if (caseId == CaseId.Empty)
                return Result.Failure<InsurancePolicy>(InsurancePolicyErrors.CaseNotSpecified);

            if (string.IsNullOrWhiteSpace(companyName))
                return Result.Failure<InsurancePolicy>(InsurancePolicyErrors.CompanyNameNotSpecified);

            var trimmedCompanyName = companyName.Trim();
            if (trimmedCompanyName.Length > MaxCompanyNameLength)
                return Result.Failure<InsurancePolicy>(InsurancePolicyErrors.CompanyNameTooLong(trimmedCompanyName.Length));

            if (string.IsNullOrWhiteSpace(policyNo))
                return Result.Failure<InsurancePolicy>(InsurancePolicyErrors.PolicyNoNotSpecified);

            var trimmedPolicyNo = policyNo.Trim();
            if (trimmedPolicyNo.Length > MaxPolicyNoLength)
                return Result.Failure<InsurancePolicy>(InsurancePolicyErrors.PolicyNoTooLong(trimmedPolicyNo.Length));

            if (coverageStartUtc is not null
                && coverageEndUtc is not null
                && coverageEndUtc.Value.Date < coverageStartUtc.Value.Date)
                return Result.Failure<InsurancePolicy>(InsurancePolicyErrors.InvalidCoverageRange);

            if (premiumRial is not null && premiumRial.Value < 0)
                return Result.Failure<InsurancePolicy>(InsurancePolicyErrors.InvalidPremium);

            if (statusId == LookupId.Empty)
                return Result.Failure<InsurancePolicy>(InsurancePolicyErrors.StatusNotSpecified);

            var policy = new InsurancePolicy(
                id, caseId, trimmedCompanyName, trimmedPolicyNo,
                coverageStartUtc?.Date, coverageEndUtc?.Date, premiumRial,
                statusId, NormalizeMeta(meta));

            policy.AddDomainEvent(new InsurancePolicyCreatedEvent(
                id.Value,
                caseId.Value,
                policy.CompanyName,
                policy.PolicyNo,
                policy.CoverageStart,
                policy.CoverageEnd,
                premiumRial,
                statusId.Value,
                policy.Meta));

            return Result.Success(policy);
        }

        // گذار وضعیت بیمه‌نامه — اعتبارسنجی ماشین وضعیت با کد lookup در لایه Application
        public Result ChangeStatus(LookupId newStatusId)
        {
            if (newStatusId == LookupId.Empty)
                return Result.Failure(InsurancePolicyErrors.StatusNotSpecified);

            if (newStatusId == StatusId)
                return Result.Failure(InsurancePolicyErrors.SameStatus);

            var oldStatusId = StatusId;
            StatusId = newStatusId;

            AddDomainEvent(new InsurancePolicyStatusChangedEvent(
                Id.Value,
                CaseId.Value,
                oldStatusId.Value,
                newStatusId.Value));

            return Result.Success();
        }

        // به‌روزرسانی بازهٔ پوشش بیمه‌نامه — هر دو با null قابل پاک‌سازی‌اند
        public Result UpdateCoverage(DateTime? coverageStartUtc, DateTime? coverageEndUtc)
        {
            if (coverageStartUtc is not null
                && coverageEndUtc is not null
                && coverageEndUtc.Value.Date < coverageStartUtc.Value.Date)
                return Result.Failure(InsurancePolicyErrors.InvalidCoverageRange);

            if (coverageStartUtc?.Date == CoverageStart?.Date
                && coverageEndUtc?.Date == CoverageEnd?.Date)
                return Result.Success();

            var oldCoverageStart = CoverageStart;
            var oldCoverageEnd = CoverageEnd;
            CoverageStart = coverageStartUtc?.Date;
            CoverageEnd = coverageEndUtc?.Date;

            AddDomainEvent(new InsurancePolicyCoverageUpdatedEvent(
                Id.Value,
                CaseId.Value,
                oldCoverageStart,
                oldCoverageEnd,
                CoverageStart,
                CoverageEnd));

            return Result.Success();
        }

        // حق بیمه (ریال) — >= 0
        public Result UpdatePremium(long? newPremiumRial)
        {
            if (newPremiumRial is not null && newPremiumRial.Value < 0)
                return Result.Failure(InsurancePolicyErrors.InvalidPremium);

            if (newPremiumRial == PremiumRial)
                return Result.Success();

            var oldPremiumRial = PremiumRial;
            PremiumRial = newPremiumRial;

            AddDomainEvent(new InsurancePolicyPremiumUpdatedEvent(
                Id.Value,
                CaseId.Value,
                oldPremiumRial,
                newPremiumRial));

            return Result.Success();
        }

        // به‌روزرسانی Meta (jsonb آزاد) — با null قابل پاک‌سازی است
        public Result SetMeta(string? meta)
        {
            var normalized = NormalizeMeta(meta);
            if (normalized == Meta)
                return Result.Failure(InsurancePolicyErrors.MetaUnchanged);

            var oldMeta = Meta;
            Meta = normalized;

            AddDomainEvent(new InsurancePolicyMetaChangedEvent(
                Id.Value,
                CaseId.Value,
                oldMeta,
                normalized));

            return Result.Success();
        }

        private static string? NormalizeMeta(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
