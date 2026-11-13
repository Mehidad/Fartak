using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Application.Contract;
using Fartak.Application.Framework.Queries;
using Fartak.Application.Role.Queries.DTO;
using Fartak.Domain.Framework;
using Microsoft.EntityFrameworkCore;

namespace Fartak.Application.Role.Queries.GetRolesQuery
{
    public sealed class GetRolesQueryHandler : IQueryHandler<GetRolesQuery, Result<IReadOnlyList<RoleListItemDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetRolesQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<IReadOnlyList<RoleListItemDto>>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
        {
            var skip = (request.PageNumber - 1) * request.PageSize;

            // ⚠️ دقت کن که حتماً مسیر کامل دامین رو بنویسی تا ارور Namespace نده
            var roles = await _context.Set<Fartak.Domain.Role.Role>()
                .AsNoTracking()
                .Where(x => !x.IsDeleted) // حذف شده‌ها را نشان نده
                .OrderByDescending(x => x.CreatedAt)
                .Skip(skip)
                .Take(request.PageSize)
                .Select(x => new RoleListItemDto
                {
                    Id = x.Id.Value,
                    Title = x.Title,
                    Code = x.Code,
                    IsSystem = x.IsSystem,
                    IsActive = x.IsActive,
                  
                })
                .ToListAsync(cancellationToken);

            return Result.Success<IReadOnlyList<RoleListItemDto>>(roles);
        }
    }
}
