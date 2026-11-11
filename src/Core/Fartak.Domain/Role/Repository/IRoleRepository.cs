using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fartak.Domain.Role.Repository
{
    public interface IRoleRepository
    {
        Task<Role?> GetByIdAsync(RoleId id, CancellationToken cancellationToken);
        Task AddAsync(Role role, CancellationToken cancellationToken);
        Task<bool> IsCodeUniqueAsync(string code, CancellationToken cancellationToken);
    }
}
