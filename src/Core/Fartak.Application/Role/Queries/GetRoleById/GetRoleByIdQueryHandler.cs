using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Application.Contract;
using Fartak.Application.Framework.Queries;
using Fartak.Application.Role.Queries.DTO;
using Fartak.Domain.Framework;
using Fartak.Domain.Role;
using Microsoft.EntityFrameworkCore;

namespace Fartak.Application.Role.Queries.GetRoleById
{
    public sealed class GetRoleByIdQueryHandler : IQueryHandler<GetRoleByIdQuery, Result<RoleDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetRoleByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<RoleDto>> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
        {
            var roleId = RoleId.From(request.Id);

            var roleDto = await _context.Set<Fartak.Domain.Role.Role>()
                .AsNoTracking()
                .Where(x => x.Id == roleId && !x.IsDeleted)
                .Select(x => new RoleDto
                {
                    Id = x.Id.Value,
                    Title = x.Title,
                    Code = x.Code,
                    Description = x.Description,
                    IsSystem = x.IsSystem,
                    IsActive = x.IsActive,
                   
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (roleDto is null)
            {
                // باید این ارور را در لایه دامین به RoleErrors اضافه کنی
                return Result.Failure<RoleDto>(RoleErrors.NotFound(request.Id));
            }

            return Result.Success(roleDto);
        }
    }
}
