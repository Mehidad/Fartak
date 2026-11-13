using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Application.Contract;
using Fartak.Application.Framework.Queries;
using Fartak.Application.Role.Queries.DTO;
using Fartak.Domain.Framework;

namespace Fartak.Application.Role.Queries.GetRolesQuery
{
    // 💡 اضافه کردن IQuery در کنار ICachedQuery
    public sealed record GetRolesQuery(
        int PageNumber = 1,
        int PageSize = 20
    ) : IQuery<Result<IReadOnlyList<RoleListItemDto>>>, ICachedQuery<Result<IReadOnlyList<RoleListItemDto>>>
    {
        public string CacheKey => $"GET_ROLES_{PageNumber}_{PageSize}";
        public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
    }
}
