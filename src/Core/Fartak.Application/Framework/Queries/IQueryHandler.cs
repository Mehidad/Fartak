using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fartak.Application.Framework.Queries
{
    public interface IQueryHandler<TQuery, TResponse>
      : MediatR.IRequestHandler<TQuery, TResponse>
      where TQuery : IQuery<TResponse>
    { }
}
