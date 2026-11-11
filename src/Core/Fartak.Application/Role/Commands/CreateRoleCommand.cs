using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Application.Framework.Commands;
using Fartak.Domain.Framework;

namespace Fartak.Application.Role.Commands
{
    public sealed record CreateRoleCommand(
        string Title,
        string Code,
        string? Description = null,
        bool IsSystem = false
    ) : ICommand<Result<long>>;
}
