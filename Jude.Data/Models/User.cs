using System.ComponentModel.DataAnnotations;

namespace Jude.Data.Models;

public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string AvatarUrl { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime? EmailConfirmedAt { get; set; } = null;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public AuthProvider AuthProvider { get; set; }
}

public enum AuthProvider
{
    Email,
}
