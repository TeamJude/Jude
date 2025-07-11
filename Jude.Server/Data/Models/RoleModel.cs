using System.ComponentModel.DataAnnotations;

namespace Jude.Server.Data.Models;

public class RoleModel
{
    [Key]
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, Permission> Permissions { get; set; } = [];
}

public enum Permission
{
    Read,
    Write,
}
