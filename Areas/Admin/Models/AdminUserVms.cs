using System.ComponentModel.DataAnnotations;

namespace HeThongQuanLyThi.Models;

public class AdminUserListItemVm
{
    public string Id { get; set; } = default!;
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
    public bool IsLockedOut { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
}

public class AdminUserCreateVm
{
    [Required, EmailAddress]
    public string Email { get; set; } = default!;

    [Required, StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = default!;

    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = default!;

    public string? FullName { get; set; }

    [Required]
    public string Role { get; set; } = "student"; // student | teacher | admin
}

public class AdminUserEditVm
{
    [Required]
    public string Id { get; set; } = default!;

    [Required, EmailAddress]
    public string Email { get; set; } = default!;

    [Required]
    public string UserName { get; set; } = default!;

    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }

    [Required]
    public string Role { get; set; } = "student";

    public bool LockoutEnabled { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
}