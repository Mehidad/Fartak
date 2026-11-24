using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Fartak.Application.Contract;
using Fartak.Domain.Framework;
using Fartak.Domain.Role;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fartak.Infrastructure.SqlServer.Shared
{
    public sealed class FartakDbContext : DbContext, IApplicationDbContext
    {
        private readonly IMediator _mediator;


        public FartakDbContext(
            DbContextOptions<FartakDbContext> options,
            IMediator? mediator = null)
            : base(options)
        {
            _mediator = mediator;
        }

        public DbSet<Fartak.Domain.Role.Role> Roles => Set<Fartak.Domain.Role.Role>();
      
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(FartakDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }

        public override async Task<int> SaveChangesAsync(
    CancellationToken cancellationToken = default)
        {
            var domainEvents = ChangeTracker
                .Entries<IHasDomainEvents>()
                .SelectMany(x => x.Entity.DomainEvents)
                .ToList();

            if (_mediator is not null)
            {
                foreach (var domainEvent in domainEvents)
                {
                    await _mediator.Publish(
                        domainEvent,
                        cancellationToken);
                }

                foreach (var entry in ChangeTracker.Entries<IHasDomainEvents>())
                {
                    entry.Entity.ClearDomainEvents();
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
