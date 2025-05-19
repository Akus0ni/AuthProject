using Microsoft.EntityFrameworkCore;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ApplicationDbContext _context;

    public RefreshTokenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task StoreRefreshTokenAsync(RefreshToken token)
    {
        _context.RefreshTokens.Add(token);
        await _context.SaveChangesAsync();
    }

    public async Task<RefreshToken?> GetValidRefreshTokenAsync(string userId, string token)
    {
        return await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.UserId == userId && t.Token == token && t.ExpiryDate > DateTime.UtcNow);
    }

    public async Task RemoveExpiredTokensAsync(string userId)
    {
        var expired = _context.RefreshTokens
            .Where(t => t.UserId == userId && t.ExpiryDate <= DateTime.UtcNow);
        _context.RefreshTokens.RemoveRange(expired);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveTokenAsync(string token, string userId)
    {
        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == token && t.UserId == userId);
        if (storedToken != null)
        {
            _context.RefreshTokens.Remove(storedToken);
            await _context.SaveChangesAsync();
        }
    }
}
