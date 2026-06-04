using System.ComponentModel.DataAnnotations;

namespace TestManagement.DAL.Models;

public class User
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Phone, MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [MaxLength(255)]
    public string? AvatarUrl { get; set; }

    [Required, MaxLength(30)]
    public string Role { get; set; } = "Student";

    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}
