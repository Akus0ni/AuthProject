using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class TokenService
{
    private readonly IConfiguration _configuration;
    private readonly SymmetricSecurityKey _key;

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IRefreshTokenRepository _refreshTokenRepo;

    public TokenService(IConfiguration config, UserManager<ApplicationUser> userManager, IRefreshTokenRepository refreshTokenRepo)
    {
        _configuration = config;
        _userManager = userManager;
        _refreshTokenRepo = refreshTokenRepo;
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
    }

    public string GenerateAccessToken(string username, string role)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role)
        };

        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(15), // Short-lived access token
            SigningCredentials = creds,
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"]
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    public async Task<string> GenerateAndStoreRefreshTokenAsync(ApplicationUser user)
    {
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var expiry = DateTime.UtcNow.AddDays(7);

        var tokenEntity = new RefreshToken
        {
            Token = refreshToken,
            ExpiryDate = expiry,
            UserId = user.Id
        };

        await _refreshTokenRepo.StoreRefreshTokenAsync(tokenEntity);

        return refreshToken;
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudience = _configuration["Jwt:Audience"],
            ValidateIssuer = true,
            ValidIssuer = _configuration["Jwt:Issuer"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _key,
            ValidateLifetime = false // ⚠️ Ignore expiration here
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }
        catch
        {
            return null;
        }
    }

    public async Task<ApplicationUser?> ValidateRefreshTokenAsync(string refreshToken, ClaimsPrincipal principal)
    {
        var username = principal.Identity?.Name;
        var user = await _userManager.FindByNameAsync(username!);

        if (user == null) return null;

        var token = await _refreshTokenRepo.GetValidRefreshTokenAsync(user.Id, refreshToken);
        return token != null ? user : null;
    }
    
    public async Task RemoveExpiredRefreshTokensAsync(string userId)
    {
        await _refreshTokenRepo.RemoveExpiredTokensAsync(userId);
    }

}
