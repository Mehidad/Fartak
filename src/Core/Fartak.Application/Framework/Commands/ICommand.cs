using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fartak.Application.Framework.Commands
{
    public interface ICommand : MediatR.IRequest { }

    public interface ICommand<TResponse> : MediatR.IRequest<TResponse> { }
}
