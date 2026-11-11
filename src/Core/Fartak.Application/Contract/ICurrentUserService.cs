using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fartak.Application.Contract
{
    public interface ICurrentUserService
    {
        long UserId { get; }
        string? UserName { get; }
        bool IsAuthenticated { get; }
    }
}
