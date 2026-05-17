namespace dWebShop.Application.Common.Interfaces;

public interface IAppDbContextFactory
{
    Task<IAppDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default);
}
