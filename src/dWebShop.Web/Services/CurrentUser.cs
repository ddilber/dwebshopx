using System.Security.Claims;
using dWebShop.Infrastructure.Persistence;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Web.Services;

// Resolves identity + partner mapping for the signed-in user in a single
// scoped service so razor pages and the shop services don't have to juggle
// AuthenticationStateProvider + UserManager themselves.
//
// Important: this uses IDbContextFactory rather than the scoped DbContext
// because Blazor static SSR renders the layout (Navbar) and the page
// concurrently. A scoped DbContext shared across both would hit
// "second operation on this context" errors. A fresh context per resolution
// keeps the resolution path isolated from anything the page is doing on the
// scoped context.
//
// Cached per-scope (per Blazor circuit / per request). The first call hits
// the DB; subsequent calls return cached values.
public class CurrentUser(
    AuthenticationStateProvider authStateProvider,
    IDbContextFactory<AppDbContext> dbFactory)
{
    private bool _resolved;
    private int? _userId;
    private int? _partnerId;
    private string? _displayName;
    private bool _isAuthenticated;

    public async Task<bool> IsAuthenticatedAsync()
    {
        await EnsureResolvedAsync();
        return _isAuthenticated;
    }

    public async Task<int?> GetUserIdAsync()
    {
        await EnsureResolvedAsync();
        return _userId;
    }

    public async Task<int?> GetPartnerIdAsync()
    {
        await EnsureResolvedAsync();
        return _partnerId;
    }

    public async Task<string?> GetDisplayNameAsync()
    {
        await EnsureResolvedAsync();
        return _displayName;
    }

    private async Task EnsureResolvedAsync()
    {
        if (_resolved) return;
        _resolved = true;

        var authState = await authStateProvider.GetAuthenticationStateAsync();
        var principal = authState.User;
        _isAuthenticated = principal.Identity?.IsAuthenticated == true;
        if (!_isAuthenticated) return;

        // ApplicationUser uses int PK. Identity stores the id in the
        // NameIdentifier claim as a string; parse defensively.
        var idClaim = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idClaim, out var uid)) return;
        _userId = uid;

        // Fresh DbContext keeps this query off the scoped context the page
        // is using concurrently. Project to just what we need so EF doesn't
        // pull the full identity row + nav collections.
        await using var db = await dbFactory.CreateDbContextAsync();
        var row = await db.Users
            .AsNoTracking()
            .Where(u => u.Id == uid)
            .Select(u => new { u.PartnerId, u.UserName, u.Email })
            .FirstOrDefaultAsync();

        if (row is null) return;
        _partnerId = row.PartnerId;
        _displayName = !string.IsNullOrWhiteSpace(row.UserName) ? row.UserName : row.Email;
    }
}
