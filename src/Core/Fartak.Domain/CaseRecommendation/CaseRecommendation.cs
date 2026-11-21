using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Case;
using Fartak.Domain.CaseRecommendation.Events;
using Fartak.Domain.FieldVisit;
using Fartak.Domain.Framework;
using Fartak.Domain.User.Fartak.Domain.User;

namespace Fartak.Domain.CaseRecommendation
{
    // توصیه/توضیح کارشناس روی پرونده (جدول case_recommendations) — تجمع مستقل بدون فرزند؛
    // می‌تواند به یک بازدید میدانی متصل باشد (visit_id اختیاری، FK به field_visits) و
    // مخاطبان اعلانش با دو فلگ boolean مشخص می‌شود (notify_manager / notify_farmer) —
    // lookup نیست، پس LookupId در این تجمع جایی ندارد.
    //
    // ستون‌های جدول: case_id، visit_id (اختیاری)، author_id، body، notify_manager،
    // notify_farmer و created_at.
    // طبق قاعده timestamp/is_active: created_at داریم ولی updated_at نداریم → فقط CreatedAt؛
    // is_active و status هم نداریم → نه Activate/Deactivate و نه ماشین وضعیت.
    //
    // تغییرناپذیرها پس از ایجاد: CaseId، AuthorId و CreatedAt؛ قابل ویرایش: متن توصیه،
    // مخاطبان اعلان و اتصال به بازدید.
    //
    // ارسال اعلان کارِ این تجمع نیست: Handler رویدادها در لایه Application مخاطبان را
    // می‌خواند و اعلان می‌فرستد؛ جلوگیری از اعلان تکراری وقتی مخاطبان بعد از ارسال اول
    // تغییر می‌کنند (idempotency) هم مسئولیت همان Handlerهاست.
    public class CaseRecommendation : AggregateRoot<CaseRecommendationId>
    {
        public const int MaxBodyLength = 4000;  // هم‌سایز یادداشت پرونده و جمع‌بندی بازدید

        public CaseId CaseId { get; private set; }          // پروندهٔ صاحب توصیه (تغییرناپذیر)
        public FieldVisitId? VisitId { get; private set; }  // بازدید مبدأ توصیه (اختیاری — SetLinkedVisit)
        public UserId AuthorId { get; private set; }        // نویسنده توصیه (کارشناس) — تغییرناپذیر
        public string Body { get; private set; }            // متن توصیه
        public bool NotifyManager { get; private set; }     // مخاطب اعلان: مدیر سیستم
        public bool NotifyFarmer { get; private set; }      // مخاطب اعلان: کشاورز
        public DateTime CreatedAt { get; private set; }

        private CaseRecommendation() { } // برای EF Core

        private CaseRecommendation(CaseRecommendationId id, CaseId caseId, UserId authorId, string body)
        {
            Id = id;
            CaseId = caseId;
            AuthorId = authorId;
            Body = body;
            CreatedAt = DateTime.UtcNow;
        }

        // ثبت توصیه جدید توسط کارشناس — اتصال به بازدید و مخاطبان اعلان اختیاری‌اند
        // (هر دو فلگ false یعنی توصیهٔ خصوصی روی پرونده، فقط در تاریخچه دیده می‌شود)
        public static Result<CaseRecommendation> Create(
            CaseRecommendationId id,
            CaseId caseId,
            UserId authorId,
            string body,
            bool notifyManager = false,
            bool notifyFarmer = false,
            FieldVisitId? visitId = null)
        {
            if (id is null || caseId is null || authorId is null)
                return Result.Failure<CaseRecommendation>(CaseRecommendationErrors.NullReferenceError());

            var bodyValidation = ValidateBody(body);
            if (bodyValidation.IsFailure)
                return Result.Failure<CaseRecommendation>(bodyValidation.Error);

            var recommendation = new CaseRecommendation(id, caseId, authorId, bodyValidation.Value);
            recommendation.VisitId = visitId;
            recommendation.NotifyManager = notifyManager;
            recommendation.NotifyFarmer = notifyFarmer;

            recommendation.AddDomainEvent(new CaseRecommendationCreatedEvent(
                id.Value, caseId.Value, authorId.Value, visitId?.Value, notifyManager, notifyFarmer));
            return Result<CaseRecommendation>.Success(recommendation);
        }

        // ویرایش متن توصیه (اصلاح نگارشی/تکمیل) — متن خالی رد می‌شود؛ تغییر نداشتن هم خطاست
        public Result EditBody(string newBody)
        {
            var validation = ValidateBody(newBody);
            if (validation.IsFailure)
                return validation;

            if (validation.Value == Body)
                return Result.Failure(CaseRecommendationErrors.BodyUnchanged());

            Body = validation.Value;
            AddDomainEvent(new CaseRecommendationBodyEditedEvent(Id.Value, CaseId.Value));
            return Result.Success();
        }

        // تعیین مخاطبان اعلان (مدیر سیستم و/یا کشاورز) — بدون محدودیت وضعیت چون status نداریم
        public Result SetNotificationAudiences(bool notifyManager, bool notifyFarmer)
        {
            if (notifyManager == NotifyManager && notifyFarmer == NotifyFarmer)
                return Result.Failure(CaseRecommendationErrors.AudiencesUnchanged());

            NotifyManager = notifyManager;
            NotifyFarmer = notifyFarmer;
            AddDomainEvent(new CaseRecommendationAudienceChangedEvent(Id.Value, notifyManager, notifyFarmer));
            return Result.Success();
        }

        // اتصال توصیه به بازدید مبدأ یا حذف اتصال (null) — در هر دو جهت اختیاری است
        public Result SetLinkedVisit(FieldVisitId? visitId)
        {
            var newVisitId = visitId?.Value;
            var oldVisitId = VisitId?.Value;
            if (newVisitId == oldVisitId)
                return Result.Failure(CaseRecommendationErrors.VisitUnchanged(visitId?.Value));

            VisitId = visitId;
            AddDomainEvent(new Events.CaseRecommendationVisitSetEvent(Id.Value, newVisitId));
            return Result.Success();
        }

        private static Result<string> ValidateBody(string? body)
        {
            if (string.IsNullOrWhiteSpace(body))
                return Result.Failure<string>(CaseRecommendationErrors.InvalidBody());

            var trimmed = body.Trim();
            if (trimmed.Length > MaxBodyLength)
                return Result.Failure<string>(CaseRecommendationErrors.BodyTooLong(trimmed.Length));

            return Result<string>.Success(trimmed);
        }
    }
}
