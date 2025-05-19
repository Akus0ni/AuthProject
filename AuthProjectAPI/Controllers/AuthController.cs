using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private static List<User> users = new()
    {
        new User { Username = "admin", Password = "admin", Role = "Admin" }
    };

    private static Dictionary<string, string> refreshTokens = new();

    private readonly TokenService _tokenService;

    public AuthController(TokenService tokenService)
    {
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] User login)
    {
        var user = users.SingleOrDefault(u => u.Username == login.Username && u.Password == login.Password);
        if (user == null) return Unauthorized();

        var accessToken = _tokenService.GenerateAccessToken(user.Username, user.Role);
        var refreshToken = _tokenService.GenerateRefreshToken();

        refreshTokens[refreshToken] = user.Username;

        return Ok(new AuthResponse { AccessToken = accessToken, RefreshToken = refreshToken });
    }

    [HttpPost("refresh")]
    public IActionResult Refresh([FromBody] AuthResponse tokens)
    {
        if (!refreshTokens.ContainsKey(tokens.RefreshToken))
            return Unauthorized("Invalid refresh token.");

        var principal = _tokenService.GetPrincipalFromExpiredToken(tokens.AccessToken);
        if (principal == null)
            return Unauthorized("Invalid access token.");

        var username = principal.Identity!.Name!;
        if (refreshTokens[tokens.RefreshToken] != username)
            return Unauthorized("Invalid refresh token-user match.");

        var newAccessToken = _tokenService.GenerateAccessToken(username, "Admin");
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        refreshTokens.Remove(tokens.RefreshToken);
        refreshTokens[newRefreshToken] = username;

        return Ok(new AuthResponse { AccessToken = newAccessToken, RefreshToken = newRefreshToken });
    }
}
