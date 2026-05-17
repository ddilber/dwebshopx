using dWebShop.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Infrastructure.Persistence;

/// <summary>
/// Adapts EF Core's IDbContextFactory to the application-layer IAppDbContextFactory,
/// creating a fresh DbContext per operation to prevent Blazor Server concurrency issues.
/// </summary>
public class AppDbContextFactoryAdapter(IDbContextFactory<AppDbContext> factory) : IAppDbContextFactory
{
    public async Task<IAppDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
        => await factory.CreateDbContextAsync(cancellationToken);
}
