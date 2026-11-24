using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Fartak.Application.Extensions.DependencyInjection
{
    public static class ApplicationRegistrar
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            var assembly = typeof(ApplicationRegistrar).Assembly;




            services.AddMediatR(c =>
            {
                c.RegisterServicesFromAssemblyContaining(typeof(ApplicationRegistrar));
                //c.AddOpenBehavior(typeof(LoggingBehavior<,>));
              //  c.AddOpenBehavior(typeof(ValidationBehavior<,>));
               // c.AddOpenBehavior(typeof(QueryCachingBehavior<,>));
               // c.AddOpenBehavior(typeof(UnitOfWorkBehavior<,>));

            });
            services.AddValidatorsFromAssembly(typeof(ApplicationRegistrar).Assembly);
            return services;
        }
    }
}
