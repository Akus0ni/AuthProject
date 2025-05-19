public interface IRefreshTokenRepository
{
    Task StoreRefreshTokenAsync(RefreshToken token);
    Task<RefreshToken?> GetValidRefreshTokenAsync(string userId, string token);
    Task RemoveExpiredTokensAsync(string userId);
    Task RemoveTokenAsync(string token, string userId);
}
