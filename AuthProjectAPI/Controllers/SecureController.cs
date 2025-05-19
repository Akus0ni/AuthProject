using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class SecureController : ControllerBase
{
    [Authorize]
    [HttpGet("data")]
    public IActionResult GetSecureData()
    {
        return Ok(new { Message = "This is a protected endpoint." });
    }
}
