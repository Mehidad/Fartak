using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Notification
{
    // خطاهای تجمع اعلان
    public static class NotificationErrors
    {
        public static readonly Error NullReference = Error.Create(
            "Notification.NullReference",
            "مقادیر اجباری اعلان ارسال نشده است.");

        public static readonly Error RecipientNotSpecified = Error.Create(
            "Notification.RecipientNotSpecified",
            "گیرندهٔ اعلان باید مشخص باشد.");

        public static readonly Error TypeNotSpecified = Error.Create(
            "Notification.TypeNotSpecified",
            "نوع اعلان باید مشخص باشد.");

        public static readonly Error TitleNotSpecified = Error.Create(
            "Notification.TitleNotSpecified",
            "عنوان اعلان نمی‌تواند خالی باشد.");

        public static Error TitleTooLong(int length)
            => Error.Create(
                code: "Notification.TitleTooLong",
                message: "عنوان اعلان نمی‌تواند بیش از ۲۰۰ نویسه باشد.",
                details: new Dictionary<string, object>
                {
                    ["Length"] = length,
                    ["MaxLength"] = Notification.MaxTitleLength
                });

        public static Error BodyTooLong(int length)
            => Error.Create(
                code: "Notification.BodyTooLong",
                message: "متن اعلان نمی‌تواند بیش از ۱۰۰۰ نویسه باشد.",
                details: new Dictionary<string, object>
                {
                    ["Length"] = length,
                    ["MaxLength"] = Notification.MaxBodyLength
                });

        public static Error RelatedEntityTypeTooLong(int length)
            => Error.Create(
                code: "Notification.RelatedEntityTypeTooLong",
                message: "نوع موجودیت مرتبط نمی‌تواند بیش از ۵۰ نویسه باشد.",
                details: new Dictionary<string, object>
                {
                    ["Length"] = length,
                    ["MaxLength"] = Notification.MaxRelatedEntityTypeLength
                });

        public static readonly Error AlreadyRead = Error.Create(
            "Notification.AlreadyRead",
            "این اعلان قبلاً خوانده شده است.");

        public static readonly Error ReadAtInFuture = Error.Create(
            "Notification.ReadAtInFuture",
            "زمان خواندن اعلان نمی‌تواند در آینده باشد.");
    }
}
