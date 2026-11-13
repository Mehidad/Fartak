using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Application.Contract;
using Fartak.Application.Framework.Queries;
using Fartak.Application.Role.Queries.DTO;
using Fartak.Domain.Framework;

namespace Fartak.Application.Role.Queries.GetRoleById
{
    public sealed record GetRoleByIdQuery(long Id)
      : IQuery<Result<RoleDto>>, ICachedQuery<Result<RoleDto>>
    {
        public string CacheKey => $"GET_ROLE_BY_ID_{Id}";
        public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
    }
}
