public class User
{
    public required string Username { get; set; }
    public required string Password { get; set; } // Store hashed in production
    public required string Role { get; set; }
}
