namespace Jude.Server.Data.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class UserModel
{
    //Id is automatically set as PK and autogen by ef
    public Guid Id { get; set; }
    public string AvatarUrl { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime? EmailConfirmedAt { get; set; } = null;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public AuthProvider AuthProvider { get; set; }

    [ForeignKey(nameof(RoleModel))]
    public Guid RoleId { get; set; }
    public required RoleModel Role { get; set; }
}

public enum AuthProvider
{
    Email,
}
