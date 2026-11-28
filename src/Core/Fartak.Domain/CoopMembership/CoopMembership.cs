using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.CoopMembership.Events;
using Fartak.Domain.Framework;
using Fartak.Domain.User;

namespace Fartak.Domain.CoopMembership
{
    // عضویت تعاونی روستایی کشاورز (جدول coop_memberships) — شماره عضویت و تاریخ؛
    // سناریو ۲ هندوور: تکمیل پروفایل کشاورز و عضویت تعاونی.
    //
    // یک کشاورز می‌تواند عضویت‌های متعددی داشته باشد؛ یکتایی (کشاورز، تعاونی)
    // بین چند تجمع است → در Application با کنترل تکرار تضمین می‌شود.
    //
    // طبق قاعده timestamp/is_active: اسکیما فقط created_at دارد و updated_at ندارد
    // → فقط CreatedAt نگه می‌شود؛ بدون UpdatedAt؛ بدون Activate/Deactivate؛
    // بدون status_id → بدون گذار وضعیت.
    public sealed class CoopMembership : AggregateRoot<CoopMembershipId>
    {
        public const int MaxCoopTitleLength = 150;      // varchar(150) NOT NULL
        public const int MaxMemberNoLength = 40;        // varchar(40) NOT NULL

        public UserId FarmerUserId { get; private set; }          // کشاورز عضو
        public string CoopTitle { get; private set; }             // نام تعاونی روستایی
        public string MemberNo { get; private set; }              // شماره عضویت
        public DateTime? JoinedAt { get; private set; }           // تاریخ عضویت (date)
        public DateTime CreatedAt { get; private set; }

        private CoopMembership() { } // برای EF Core

        private CoopMembership(
            CoopMembershipId id,
            UserId farmerUserId,
            string coopTitle,
            string memberNo,
            DateTime? joinedAt)
            : base(id)
        {
            FarmerUserId = farmerUserId;
            CoopTitle = coopTitle;
            MemberNo = memberNo;
            JoinedAt = joinedAt;
            CreatedAt = DateTime.UtcNow;
        }

        public static Result<CoopMembership> Create(
            CoopMembershipId id,
            UserId farmerUserId,
            string coopTitle,
            string memberNo,
            DateTime? joinedAtUtc = null)
        {
            if (id is null)
                return Result.Failure<CoopMembership>(CoopMembershipErrors.NullReference);

            if (farmerUserId == UserId.Empty)
                return Result.Failure<CoopMembership>(CoopMembershipErrors.FarmerNotSpecified);

            if (string.IsNullOrWhiteSpace(coopTitle))
                return Result.Failure<CoopMembership>(CoopMembershipErrors.CoopTitleNotSpecified);

            var trimmedCoopTitle = coopTitle.Trim();
            if (trimmedCoopTitle.Length > MaxCoopTitleLength)
                return Result.Failure<CoopMembership>(CoopMembershipErrors.CoopTitleTooLong(trimmedCoopTitle.Length));

            if (string.IsNullOrWhiteSpace(memberNo))
                return Result.Failure<CoopMembership>(CoopMembershipErrors.MemberNoNotSpecified);

            var trimmedMemberNo = memberNo.Trim();
            if (trimmedMemberNo.Length > MaxMemberNoLength)
                return Result.Failure<CoopMembership>(CoopMembershipErrors.MemberNoTooLong(trimmedMemberNo.Length));

            if (joinedAtUtc is not null && joinedAtUtc.Value.Date > DateTime.UtcNow.Date)
                return Result.Failure<CoopMembership>(CoopMembershipErrors.JoinedAtInFuture);

            var membership = new CoopMembership(
                id, farmerUserId, trimmedCoopTitle, trimmedMemberNo, joinedAtUtc?.Date);

            membership.AddDomainEvent(new CoopMembershipCreatedEvent(
                id.Value,
                farmerUserId.Value,
                membership.CoopTitle,
                membership.MemberNo,
                membership.JoinedAt));

            return Result.Success(membership);
        }

        // اصلاح نام تعاونی (مثلاً تغییر نام رسمی تعاونی)
        public Result SetCoopTitle(string newCoopTitle)
        {
            if (string.IsNullOrWhiteSpace(newCoopTitle))
                return Result.Failure(CoopMembershipErrors.CoopTitleNotSpecified);

            var trimmedCoopTitle = newCoopTitle.Trim();
            if (trimmedCoopTitle.Length > MaxCoopTitleLength)
                return Result.Failure(CoopMembershipErrors.CoopTitleTooLong(trimmedCoopTitle.Length));

            if (trimmedCoopTitle == CoopTitle)
                return Result.Failure(CoopMembershipErrors.CoopTitleUnchanged);

            var oldCoopTitle = CoopTitle;
            CoopTitle = trimmedCoopTitle;

            AddDomainEvent(new CoopMembershipCoopTitleChangedEvent(
                Id.Value,
                FarmerUserId.Value,
                oldCoopTitle,
                trimmedCoopTitle));

            return Result.Success();
        }

        // اصلاح شماره عضویت
        public Result SetMemberNo(string newMemberNo)
        {
            if (string.IsNullOrWhiteSpace(newMemberNo))
                return Result.Failure(CoopMembershipErrors.MemberNoNotSpecified);

            var trimmedMemberNo = newMemberNo.Trim();
            if (trimmedMemberNo.Length > MaxMemberNoLength)
                return Result.Failure(CoopMembershipErrors.MemberNoTooLong(trimmedMemberNo.Length));

            if (trimmedMemberNo == MemberNo)
                return Result.Failure(CoopMembershipErrors.MemberNoUnchanged);

            var oldMemberNo = MemberNo;
            MemberNo = trimmedMemberNo;

            AddDomainEvent(new CoopMembershipMemberNoChangedEvent(
                Id.Value,
                FarmerUserId.Value,
                oldMemberNo,
                trimmedMemberNo));

            return Result.Success();
        }

        // اصلاح تاریخ عضویت — نباید در آینده باشد؛ با null قابل پاک‌سازی است
        public Result SetJoinedAt(DateTime? newJoinedAtUtc)
        {
            if (newJoinedAtUtc is not null && newJoinedAtUtc.Value.Date > DateTime.UtcNow.Date)
                return Result.Failure(CoopMembershipErrors.JoinedAtInFuture);

            if (newJoinedAtUtc?.Date == JoinedAt?.Date)
                return Result.Failure(CoopMembershipErrors.JoinedAtUnchanged);

            var oldJoinedAt = JoinedAt;
            JoinedAt = newJoinedAtUtc?.Date;

            AddDomainEvent(new CoopMembershipJoinedAtChangedEvent(
                Id.Value,
                FarmerUserId.Value,
                oldJoinedAt,
                JoinedAt));

            return Result.Success();
        }
    }
}
