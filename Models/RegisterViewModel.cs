using System.ComponentModel.DataAnnotations;

namespace HeThongQuanLyThi.Models;

public class RegisterViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = default!;

    [Required, StringLength(100, MinimumLength = 6), DataType(DataType.Password)]
    public string Password { get; set; } = default!;

    [Compare(nameof(Password)), DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = default!;

    // Thêm thuộc tính này
    public string? ReturnUrl { get; set; }
}
