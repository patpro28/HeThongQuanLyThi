using Microsoft.AspNetCore.Identity;

namespace HeThongQuanLyThi.Services;
public interface IAuthService
{
    Task<SignInResult> PasswordSignInAsync(string email, string password, bool rememberMe);
    Task SignOutAsync();
}

public class AuthService(SignInManager<IdentityUser> signInManager) : IAuthService
{
    private readonly SignInManager<IdentityUser> _signInManager = signInManager;

    public Task<SignInResult> PasswordSignInAsync(string email, string password, bool rememberMe) =>
        _signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: false);

    public Task SignOutAsync() => _signInManager.SignOutAsync();
}
