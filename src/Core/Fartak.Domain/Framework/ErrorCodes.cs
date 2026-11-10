using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fartak.Domain.Framework
{
    public static class ErrorCodes
    {
        #region General
        public const string NotFound = "NOT_FOUND";
        public const string InvalidOperation = "INVALID_OPERATION";
        public const string InvalidId = "INVALID_ID";
        #endregion

        #region User
        public const string NullOtp = "USER_NULL_OTP";
        public const string InvalidOtp = "USER_INVALID_OTP";
        public const string EmptyFullName = "USER_EMPTY_FULL_NAME";
        public const string InvalidRoleId = "USER_INVALID_ROLE_ID";
        public const string RoleAlreadyAssigned = "USER_ROLE_ALREADY_ASSIGNED";
        #endregion

        #region Role
        public const string RoleEmptyTitle = "ROLE_EMPTY_TITLE";
        public const string RoleEmptyCode = "ROLE_EMPTY_CODE";
        public const string RoleInvalidCodeFormat = "ROLE_INVALID_CODE_FORMAT";
        public const string RoleNotFound = "ROLE_NOT_FOUND";
        public const string RoleDeleted = "ROLE_DELETED";
        public const string RoleAlreadyActive = "ROLE_ALREADY_ACTIVE";
        public const string RoleAlreadyDeactivated = "ROLE_ALREADY_DEACTIVATED";
        public const string RoleSystemCannotBeDeleted = "ROLE_SYSTEM_CANNOT_BE_DELETED";
        public const string RoleSystemCannotBeDeactivated = "ROLE_SYSTEM_CANNOT_BE_DEACTIVATED";
        #endregion



    }
}
