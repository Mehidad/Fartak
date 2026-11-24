using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Case;
using Fartak.Domain.Common;
using Fartak.Domain.Framework;
using Fartak.Domain.Notification.Events;
using Fartak.Domain.User;

namespace Fartak.Domain.Notification
{
    // اعلان (جدول notifications) — پیام‌های سیستمی به کاربر (یادآور بازدید، تغییر وضعیت پرونده،
    // هشدار هواشناسی و ...) با پیوند اختیاری به پرونده یا هر موجودیت دیگر
    // (RelatedEntityType + RelatedEntityId — همان الگوی ارجاع عمومی).
    //
    // اعلان write-once است: پس از Create تنها تغییر مجاز، «خوانده‌شدن» است (MarkRead)؛
    // ویرایش متن نداریم. گارد «خوانده‌شده» با پرچم دامنه‌ای IsRead کنترل می‌شود، نه کد lookup.
    //
    // طبق قاعده timestamp/is_active: بدون updated_at → بدون UpdatedAt؛
    // بدون is_active → بدون Activate/Deactivate؛ حذف اعلان‌های قدیمی وظیفهٔ Application است.
    public sealed class Notification : AggregateRoot<NotificationId>
    {
        public const int MaxTitleLength = 200;
        public const int MaxBodyLength = 1000;
        public const int MaxRelatedEntityTypeLength = 50;

        public UserId RecipientId { get; private set; }              // گیرندهٔ اعلان
        public string Title { get; private set; }                     // عنوان کوتاه اعلان
        public string? Body { get; private set; }                     // متن کامل (اختیاری)
        public LookupId TypeId { get; private set; }                  // lookup: notification_type
        public CaseId? CaseId { get; private set; }                   // پروندهٔ مرتبط (اختیاری)
        public string? RelatedEntityType { get; private set; }        // نوع موجودیت مرتبط (مثل PlanVersion)
        public long? RelatedEntityId { get; private set; }            // شناسهٔ موجودیت مرتبط
        public bool IsRead { get; private set; }
        public DateTime? ReadAt { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private Notification() { } // برای EF Core

        private Notification(
            NotificationId id,
            UserId recipientId,
            string title,
            string? body,
            LookupId typeId,
            CaseId? caseId,
            string? relatedEntityType,
            long? relatedEntityId)
            : base(id)
        {
            RecipientId = recipientId;
            Title = title;
            Body = body;
            TypeId = typeId;
            CaseId = caseId;
            RelatedEntityType = relatedEntityType;
            RelatedEntityId = relatedEntityId;
            CreatedAt = DateTime.UtcNow;
        }

        public static Result<Notification> Create(
            NotificationId id,
            UserId recipientId,
            LookupId typeId,
            string title,
            string? body,
            CaseId? caseId,
            string? relatedEntityType,
            long? relatedEntityId)
        {
            if (id is null)
                return Result.Failure<Notification>(NotificationErrors.NullReference);

            if (recipientId == UserId.Empty)
                return Result.Failure<Notification>(NotificationErrors.RecipientNotSpecified);

            if (typeId == LookupId.Empty)
                return Result.Failure<Notification>(NotificationErrors.TypeNotSpecified);

            if (string.IsNullOrWhiteSpace(title))
                return Result.Failure<Notification>(NotificationErrors.TitleNotSpecified);

            if (title.Trim().Length > MaxTitleLength)
                return Result.Failure<Notification>(NotificationErrors.TitleTooLong(title.Trim().Length));

            var trimmedBody = NormalizeText(body);
            if (trimmedBody is not null && trimmedBody.Length > MaxBodyLength)
                return Result.Failure<Notification>(NotificationErrors.BodyTooLong(trimmedBody.Length));

            var trimmedRelatedEntityType = NormalizeText(relatedEntityType);
            if (trimmedRelatedEntityType is not null && trimmedRelatedEntityType.Length > MaxRelatedEntityTypeLength)
                return Result.Failure<Notification>(NotificationErrors.RelatedEntityTypeTooLong(trimmedRelatedEntityType.Length));

            var notification = new Notification(
                id,
                recipientId,
                title.Trim(),
                trimmedBody,
                typeId,
                caseId,
                trimmedRelatedEntityType,
                relatedEntityId);

            notification.AddDomainEvent(new NotificationCreatedEvent(
                id.Value,
                recipientId.Value,
                typeId.Value,
                notification.Title,
                trimmedBody,
                caseId?.Value,
                trimmedRelatedEntityType,
                relatedEntityId));

            return Result.Success(notification);
        }

        // خواندن اعلان — یک‌بار برای همیشه؛ خواندن مجدد خطا می‌دهد
        public Result MarkRead(DateTime readAtUtc)
        {
            if (IsRead)
                return Result.Failure(NotificationErrors.AlreadyRead);

            if (readAtUtc > DateTime.UtcNow)
                return Result.Failure(NotificationErrors.ReadAtInFuture);

            IsRead = true;
            ReadAt = readAtUtc;

            AddDomainEvent(new NotificationReadEvent(
                Id.Value,
                RecipientId.Value,
                ReadAt.Value));

            return Result.Success();
        }

        private static string? NormalizeText(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
