using System.ComponentModel.DataAnnotations;

namespace HeThongQuanLyThi.Models;

public class LoginViewModel
{
    [Required]
    public string UserNameOrEmail { get; set; } = "";

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = "";

    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}
