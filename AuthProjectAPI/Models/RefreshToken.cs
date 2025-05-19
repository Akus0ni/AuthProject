public class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser User { get; set; } = null!;
}
