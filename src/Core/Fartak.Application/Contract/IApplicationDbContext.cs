using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Fartak.Application.Contract
{
    public interface IApplicationDbContext
    {
        DbSet<T> Set<T>() where T : class;
    }
}
