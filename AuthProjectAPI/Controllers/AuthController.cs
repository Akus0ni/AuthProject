using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly TokenService _tokenService;

    private readonly IRefreshTokenRepository _refreshTokenRepo;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        TokenService tokenService,
        IRefreshTokenRepository refreshTokenRepository) // inject context
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _refreshTokenRepo = refreshTokenRepository;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] User model)
    {
        var user = new ApplicationUser { UserName = model.Username/*, Email = model.Email*/ };
        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded) return BadRequest(result.Errors);

        // Optional: assign default role
        await _userManager.AddToRoleAsync(user, "User");

        return Ok("User registered successfully.");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] User model)
    {
        var user = await _userManager.FindByNameAsync(model.Username);
        if (user == null) return Unauthorized("Invalid username or password.");

        var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
        if (!result.Succeeded) return Unauthorized("Invalid username or password.");

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _tokenService.GenerateAccessToken(user.UserName, roles.FirstOrDefault() ?? "User");
        var refreshToken = await _tokenService.GenerateAndStoreRefreshTokenAsync(user);

        await _tokenService.RemoveExpiredRefreshTokensAsync(user.Id);

        return Ok(new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] AuthResponse tokens)
    {
        var principal = _tokenService.GetPrincipalFromExpiredToken(tokens.AccessToken);
        if (principal == null)
            return Unauthorized("Invalid access token.");
        var user = _tokenService.ValidateRefreshTokenAsync(tokens.RefreshToken, principal).Result;
        if (user == null)
            return Unauthorized("Invalid refresh token.");

        var roles = await _userManager.GetRolesAsync(user);
        var newAccessToken = _tokenService.GenerateAccessToken(user.UserName!, roles.FirstOrDefault() ?? "User");
        var newRefreshToken = await _tokenService.GenerateAndStoreRefreshTokenAsync(user);

        await _tokenService.RemoveExpiredRefreshTokensAsync(user.Id);        

        return Ok(new AuthResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] string refreshToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized("Invalid user.");

        await _refreshTokenRepo.RemoveTokenAsync(refreshToken, userId);
        return Ok("Logout successful. Refresh token revoked.");
    }
}
