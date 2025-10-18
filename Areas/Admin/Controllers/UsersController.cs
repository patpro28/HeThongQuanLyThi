using HeThongQuanLyThi.Data;
using HeThongQuanLyThi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongQuanLyThi.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "admin")]
public class UsersController(UserManager<IdentityUser> users, RoleManager<IdentityRole> roles, AppDbContext db) : Controller
{
    private readonly UserManager<IdentityUser> _users = users;
    private readonly RoleManager<IdentityRole> _roles = roles;
    private readonly AppDbContext _db = db;

    // GET: /Admin/Users
    public async Task<IActionResult> Index(string? q = null)
    {
        var query = _users.Users.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(u => (u.Email ?? "").Contains(q) || (u.UserName ?? "").Contains(q));

        var users = await query.OrderBy(u => u.Email).Take(500).ToListAsync();

        // lấy roles qua join để tránh N+1
        var userIds = users.Select(u => u.Id).ToList();
        var ur = await _db.UserRoles.Where(x => userIds.Contains(x.UserId)).ToListAsync();
        var roleIds = ur.Select(x => x.RoleId).Distinct().ToList();
        var allRoles = await _db.Roles.Where(r => roleIds.Contains(r.Id)).ToListAsync();

        var list = users.Select(u =>
        {
            var rids = ur.Where(x => x.UserId == u.Id).Select(x => x.RoleId).ToList();
            var rnames = allRoles.Where(r => rids.Contains(r.Id)).Select(r => r.Name!).ToList();
            return new AdminUserListItemVm
            {
                Id = u.Id,
                Email = u.Email,
                UserName = u.UserName,
                Roles = rnames,
                IsLockedOut = u.LockoutEnd.HasValue && u.LockoutEnd > DateTimeOffset.UtcNow,
                LockoutEnd = u.LockoutEnd
            };
        }).ToList();

        ViewBag.Query = q;
        return View(list);
    }

    // GET: /Admin/Users/Details/{id}
    public async Task<IActionResult> Details(string id)
    {
        var u = await _users.FindByIdAsync(id);
        if (u == null) return NotFound();
        var roles = await _users.GetRolesAsync(u);

        var p = await _db.Profiles.Include(x => x.Student).Include(x => x.Teacher)
                                  .FirstOrDefaultAsync(x => x.UserId == u.Id);

        ViewBag.Roles = roles;
        return View((u, p));
    }

    // GET: /Admin/Users/Create
    public IActionResult Create() => View(new AdminUserCreateVm());

    // POST: /Admin/Users/Create
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminUserCreateVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = new IdentityUser { UserName = vm.Email, Email = vm.Email, EmailConfirmed = true };
        var res = await _users.CreateAsync(user, vm.Password);
        if (!res.Succeeded)
        {
            foreach (var e in res.Errors) ModelState.AddModelError("", e.Description);
            return View(vm);
        }

        if (!await _roles.RoleExistsAsync(vm.Role))
            ModelState.AddModelError("", $"Role '{vm.Role}' không tồn tại.");
        else
            await _users.AddToRoleAsync(user, vm.Role);

        // tạo Profile mặc định
        var prof = new Profile { UserId = user.Id, FullName = vm.FullName ?? (user.UserName ?? user.Email ?? "") };
        if (vm.Role == "student") prof.Student = new StudentInfo();
        _db.Profiles.Add(prof);
        await _db.SaveChangesAsync();

        TempData["ok"] = "Đã tạo người dùng.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Admin/Users/Edit/{id}
    public async Task<IActionResult> Edit(string id)
    {
        var u = await _users.FindByIdAsync(id);
        if (u == null) return NotFound();

        var roles = await _users.GetRolesAsync(u);
        var p = await _db.Profiles.FirstOrDefaultAsync(x => x.UserId == u.Id);

        var vm = new AdminUserEditVm
        {
            Id = u.Id,
            Email = u.Email!,
            UserName = u.UserName!,
            PhoneNumber = u.PhoneNumber,
            FullName = p?.FullName,
            Role = roles.FirstOrDefault() ?? "student",
            LockoutEnabled = u.LockoutEnabled,
            LockoutEnd = u.LockoutEnd
        };

        ViewBag.AllRoles = await _roles.Roles.Select(r => r.Name!).ToListAsync();
        return View(vm);
    }

    // POST: /Admin/Users/Edit/{id}
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, AdminUserEditVm vm)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid)
        {
            ViewBag.AllRoles = await _roles.Roles.Select(r => r.Name!).ToListAsync();
            return View(vm);
        }

        var u = await _users.FindByIdAsync(id);
        if (u == null) return NotFound();

        // cập nhật IdentityUser
        u.Email = vm.Email;
        u.UserName = vm.UserName;
        u.PhoneNumber = vm.PhoneNumber;
        u.LockoutEnabled = vm.LockoutEnabled;
        u.LockoutEnd = vm.LockoutEnd;

        var updateRes = await _users.UpdateAsync(u);
        if (!updateRes.Succeeded)
        {
            foreach (var e in updateRes.Errors) ModelState.AddModelError("", e.Description);
            ViewBag.AllRoles = await _roles.Roles.Select(r => r.Name!).ToListAsync();
            return View(vm);
        }

        // cập nhật Profile (nếu có)
        var p = await _db.Profiles.FirstOrDefaultAsync(x => x.UserId == u.Id);
        if (p != null)
        {
            p.FullName = vm.FullName ?? p.FullName;
            await _db.SaveChangesAsync();
        }

        // cập nhật Role (đơn vai trò)
        var currentRoles = await _users.GetRolesAsync(u);
        if (!currentRoles.Contains(vm.Role))
        {
            // ngăn tự hạ admin cuối cùng hoặc tự tước admin của mình
            if (currentRoles.Contains("admin") && !vm.Role.Equals("admin", StringComparison.OrdinalIgnoreCase))
            {
                var selfId = _users.GetUserId(User);
                var adminsLeft = await CountOtherAdminsAsync(u.Id);
                if (u.Id == selfId || adminsLeft == 0)
                {
                    ModelState.AddModelError("", "Không thể tước quyền admin của chính bạn hoặc admin cuối cùng.");
                    ViewBag.AllRoles = await _roles.Roles.Select(r => r.Name!).ToListAsync();
                    return View(vm);
                }
            }

            await _users.RemoveFromRolesAsync(u, currentRoles);
            if (await _roles.RoleExistsAsync(vm.Role))
                await _users.AddToRoleAsync(u, vm.Role);
        }

        TempData["ok"] = "Đã cập nhật người dùng.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Admin/Users/Delete/{id}
    public async Task<IActionResult> Delete(string id)
    {
        var u = await _users.FindByIdAsync(id);
        if (u == null) return NotFound();
        var roles = await _users.GetRolesAsync(u);
        ViewBag.Roles = roles;
        return View(u);
    }

    // POST: /Admin/Users/Delete/{id}
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var u = await _users.FindByIdAsync(id);
        if (u == null) return NotFound();

        // không xoá chính mình
        if (u.Id == _users.GetUserId(User))
        {
            TempData["err"] = "Không thể xoá tài khoản đang đăng nhập.";
            return RedirectToAction(nameof(Index));
        }

        // không xoá admin cuối cùng
        var roles = await _users.GetRolesAsync(u);
        if (roles.Contains("admin"))
        {
            var others = await CountOtherAdminsAsync(u.Id);
            if (others == 0)
            {
                TempData["err"] = "Không thể xoá admin cuối cùng.";
                return RedirectToAction(nameof(Index));
            }
        }

        // xoá profile (cascade nếu có FK); ở đây xoá tay
        var profile = await _db.Profiles.FirstOrDefaultAsync(x => x.UserId == u.Id);
        if (profile != null)
        {
            _db.Profiles.Remove(profile);
            await _db.SaveChangesAsync();
        }

        var res = await _users.DeleteAsync(u);
        TempData[res.Succeeded ? "ok" : "err"] = res.Succeeded ? "Đã xoá người dùng." : string.Join("; ", res.Errors.Select(e => e.Description));
        return RedirectToAction(nameof(Index));
    }

    private async Task<int> CountOtherAdminsAsync(string exceptUserId)
    {
        var adminRole = await _roles.FindByNameAsync("admin");
        if (adminRole == null) return 0;
        var count = await _db.UserRoles.CountAsync(ur => ur.RoleId == adminRole.Id && ur.UserId != exceptUserId);
        return count;
    }
}