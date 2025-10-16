using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using HeThongQuanLyThi.Models;
using HeThongQuanLyThi.Data;
using Microsoft.EntityFrameworkCore;

namespace HeThongQuanLyThi.Controllers;

[Route("account")]
public class AccountController(UserManager<IdentityUser> users, SignInManager<IdentityUser> signIn) : Controller
{
    private readonly UserManager<IdentityUser> _users = users;
    private readonly SignInManager<IdentityUser> _signIn = signIn;

    // ==== REGISTER ====
    [HttpGet("register")]
    [AllowAnonymous]
    public IActionResult Register(string? returnUrl = null)
        => View(new RegisterViewModel { ReturnUrl = returnUrl });

    [HttpPost("register")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        // Username dùng Email để đơn giản hoá
        var user = new IdentityUser { UserName = model.Email.Trim(), Email = model.Email.Trim() };
        var result = await _users.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            await _users.AddToRoleAsync(user, "student");

            // Tạo profile mặc định (rỗng)
            using (var scope = HttpContext.RequestServices.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var profile = await db.Profiles.FirstOrDefaultAsync(x => x.UserId == user.Id);
                if (profile == null)
                {
                    profile = new Profile
                    {
                        UserId = user.Id,
                        FullName = user.UserName ?? user.Email ?? "", // tạm thời
                                                                      // có thể để null DateOfBirth/Gender/Phone...
                        Student = new StudentInfo() // rỗng
                    };
                    db.Profiles.Add(profile);
                    await db.SaveChangesAsync();
                }
            }
            // Nếu bật xác thực email: gửi mail & yêu cầu confirm trước khi sign-in
            // Ở đây sign-in luôn cho đơn giản
            await _signIn.SignInAsync(user, isPersistent: false);

            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            return RedirectToAction("Index", "Home");
        }

        foreach (var err in result.Errors)
            ModelState.AddModelError("", err.Description);

        return View(model);
    }

    // ==== LOGIN ====
    [HttpGet("login")]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
        => View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost("login")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var input = model.UserNameOrEmail.Trim();

        // Thử tìm theo Email trước; không thấy thì tìm theo Username
        var user = await _users.FindByEmailAsync(input) ?? await _users.FindByNameAsync(input);
        if (user == null)
        {
            ModelState.AddModelError("", "Sai thông tin đăng nhập.");
            return View(model);
        }

        var result = await _signIn.PasswordSignInAsync(
            user, model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);
            return RedirectToAction("Index", "Home");
        }

        if (result.IsLockedOut)
            ModelState.AddModelError("", "Tài khoản tạm khoá do đăng nhập sai nhiều lần.");
        else if (result.RequiresTwoFactor)
            ModelState.AddModelError("", "Tài khoản yêu cầu xác thực 2 lớp (2FA).");
        else
            ModelState.AddModelError("", "Sai thông tin đăng nhập.");

        return View(model);
    }

    // ==== LOGOUT ====
    [Authorize]
    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signIn.SignOutAsync();
        return RedirectToAction("Login");
    }

    [HttpGet("denied")]
    [AllowAnonymous]
    public IActionResult Denied() => View();
}
