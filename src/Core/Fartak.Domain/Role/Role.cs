using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;
using Fartak.Domain.Role.Events;

namespace Fartak.Domain.Role
{
    public class Role : AggregateRoot<RoleId>
    {
        // اطلاعات اصلی
        public string Title { get; private set; }      // نام نمایشی: کشاورز، کارشناس کشاورزی، منتور
        public string Code { get; private set; }       // کد یکتا برای چک‌های برنامه‌ای: Farmer, Expert, Mentor
        public string? Description { get; private set; }
        public bool IsSystem { get; private set; }     // نقش‌های سیستمی (پایه) حذف و غیرفعال نمی‌شوند

        // وضعیت
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public bool IsDeleted { get; private set; }

        private Role() { } // برای EF Core

        private Role(RoleId id, string title, string code, string? description, bool isSystem)
        {
            Id = id;
            Title = title;
            Code = code;
            Description = description;
            IsSystem = isSystem;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            IsDeleted = false;
        }

        public static Result<Role> Create(
            RoleId id,
            string title,
            string code,
            string? description = null,
            bool isSystem = false)
        {
            if (string.IsNullOrWhiteSpace(title))
                return Result.Failure<Role>(RoleErrors.EmptyTitle());

            if (string.IsNullOrWhiteSpace(code))
                return Result.Failure<Role>(RoleErrors.EmptyCode());

            if (!IsValidCode(code))
                return Result.Failure<Role>(RoleErrors.InvalidCodeFormat(code));

            var role = new Role(id, title.Trim(), code.Trim().ToUpperInvariant(), description?.Trim(), isSystem);
            role.AddDomainEvent(new RoleCreatedEvent(id.Value, role.Title, role.Code));
            return Result<Role>.Success(role);
        }

        // متدهای Domain
        public Result UpdateDetails(string title, string? description)
        {
            if (IsDeleted)
                return Result.Failure(RoleErrors.RoleDeleted(Id.Value));

            if (string.IsNullOrWhiteSpace(title))
                return Result.Failure(RoleErrors.EmptyTitle());

            Title = title.Trim();
            Description = description?.Trim();

            AddDomainEvent(new RoleUpdatedEvent(Id.Value));
            return Result.Success();
        }

        public Result Deactivate()
        {
            if (IsDeleted)
                return Result.Failure(RoleErrors.RoleDeleted(Id.Value));

            if (IsSystem)
                return Result.Failure(RoleErrors.SystemRoleCannotBeDeactivated(Id.Value));

            if (!IsActive)
                return Result.Failure(RoleErrors.RoleAlreadyDeactivated(Id.Value));

            IsActive = false;
            AddDomainEvent(new RoleDeactivatedEvent(Id.Value));
            return Result.Success();
        }

        public Result Activate()
        {
            if (IsDeleted)
                return Result.Failure(RoleErrors.RoleDeleted(Id.Value));

            if (IsActive)
                return Result.Failure(RoleErrors.RoleAlreadyActive(Id.Value));

            IsActive = true;
            AddDomainEvent(new RoleUpdatedEvent(Id.Value));
            return Result.Success();
        }

        public Result Delete()
        {
            if (IsDeleted)
                return Result.Failure(RoleErrors.RoleDeleted(Id.Value));

            if (IsSystem)
                return Result.Failure(RoleErrors.SystemRoleCannotBeDeleted(Id.Value));

            IsDeleted = true;
            AddDomainEvent(new RoleDeletedEvent(Id.Value));
            return Result.Success();
        }

        public static bool IsValidCode(string code) =>
            code.All(c => (char.IsLetterOrDigit(c) && c <= 127) || c == '_');
    }
}
