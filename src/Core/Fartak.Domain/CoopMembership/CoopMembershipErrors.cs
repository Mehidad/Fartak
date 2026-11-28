using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.CoopMembership
{
    // خطاهای تجمع عضویت تعاونی
    public static class CoopMembershipErrors
    {
        public static readonly Error NullReference = Error.Create(
            "CoopMembership.NullReference",
            "شناسهٔ عضویت تعاونی ارسال نشده است.");

        public static readonly Error FarmerNotSpecified = Error.Create(
            "CoopMembership.FarmerNotSpecified",
            "برای ثبت عضویت تعاونی باید کشاورز مشخص باشد.");

        public static readonly Error CoopTitleNotSpecified = Error.Create(
            "CoopMembership.CoopTitleNotSpecified",
            "نام تعاونی باید مشخص باشد.");

        public static Error CoopTitleTooLong(int length)
            => Error.Create(
                code: "CoopMembership.CoopTitleTooLong",
                message: "نام تعاونی نمی‌تواند بیش از ۱۵۰ نویسه باشد.",
                details: new Dictionary<string, object>
                {
                    ["Length"] = length,
                    ["MaxLength"] = CoopMembership.MaxCoopTitleLength
                });

        public static readonly Error MemberNoNotSpecified = Error.Create(
            "CoopMembership.MemberNoNotSpecified",
            "شماره عضویت باید مشخص باشد.");

        public static Error MemberNoTooLong(int length)
            => Error.Create(
                code: "CoopMembership.MemberNoTooLong",
                message: "شماره عضویت نمی‌تواند بیش از ۴۰ نویسه باشد.",
                details: new Dictionary<string, object>
                {
                    ["Length"] = length,
                    ["MaxLength"] = CoopMembership.MaxMemberNoLength
                });

        public static readonly Error JoinedAtInFuture = Error.Create(
            "CoopMembership.JoinedAtInFuture",
            "تاریخ عضویت نمی‌تواند در آینده باشد.");

        public static readonly Error CoopTitleUnchanged = Error.Create(
            "CoopMembership.CoopTitleUnchanged",
            "نام تعاونی با مقدار فعلی یکسان است.");

        public static readonly Error MemberNoUnchanged = Error.Create(
            "CoopMembership.MemberNoUnchanged",
            "شماره عضویت با مقدار فعلی یکسان است.");

        public static readonly Error JoinedAtUnchanged = Error.Create(
            "CoopMembership.JoinedAtUnchanged",
            "تاریخ عضویت با مقدار فعلی یکسان است.");
    }
}
