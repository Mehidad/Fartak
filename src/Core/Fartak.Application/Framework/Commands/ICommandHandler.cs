using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fartak.Application.Framework.Commands
{
    public interface ICommandHandler<TCommand>
     : MediatR.IRequestHandler<TCommand>
     where TCommand : ICommand
    { }

    public interface ICommandHandler<TCommand, TResponse>
        : MediatR.IRequestHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    { }
}
