using HeThongQuanLyThi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HeThongQuanLyThi.Models;

namespace HeThongQuanLyThi.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "admin")]
public class TeachersController(UserManager<IdentityUser> users, AppDbContext db) : Controller
{
    private readonly UserManager<IdentityUser> _users = users;
    private readonly AppDbContext _db = db;

    // Danh sách profile để promote
    public async Task<IActionResult> Index(string? q = null)
    {
        var query = _db.Profiles
            .Include(p => p.Student)
            .Include(p => p.Teacher)
            .Select(p => new TeacherPromoteVm
            {
                UserId = p.UserId,
                FullName = p.FullName,
                HasStudent = p.Student != null,
                HasTeacher = p.Teacher != null
            });

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(x => x.FullName!.Contains(q));

        var list = await query.OrderBy(x => x.FullName).Take(200).ToListAsync();
        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> Promote(string userId)
    {
        var user = await _users.FindByIdAsync(userId);
        if (user == null) return NotFound();

        var profile = await _db.Profiles.Include(x => x.Teacher).FirstOrDefaultAsync(x => x.UserId == userId);
        if (profile == null) return NotFound("Profile not found.");

        var vm = new PromoteTeacherVm
        {
            UserId = userId,
            FullName = profile.FullName,
            Subject = profile.Teacher?.Subject,
            Degree = profile.Teacher?.Degree,
            HomeroomClass = profile.Teacher?.HomeroomClass
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Promote(PromoteTeacherVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = await _users.FindByIdAsync(vm.UserId);
        if (user == null) return NotFound();

        // Set role teacher, bỏ role student
        if (!await _users.IsInRoleAsync(user, "teacher"))
            await _users.AddToRoleAsync(user, "teacher");
        if (await _users.IsInRoleAsync(user, "student"))
            await _users.RemoveFromRoleAsync(user, "student");

        var profile = await _db.Profiles
            .Include(x => x.Student)
            .Include(x => x.Teacher)
            .FirstOrDefaultAsync(x => x.UserId == vm.UserId);

        if (profile == null) return NotFound("Profile not found.");

        // Chỉ giữ nhánh Teacher
        profile.Student = null;
        profile.Teacher ??= new TeacherInfo();
        profile.Teacher.Subject = vm.Subject;
        profile.Teacher.Degree = vm.Degree;
        profile.Teacher.HomeroomClass = vm.HomeroomClass;

        await _db.SaveChangesAsync();
        TempData["ok"] = "Đã gán giáo viên.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Demote(string userId)
    {
        var user = await _users.FindByIdAsync(userId);
        if (user == null) return NotFound();

        if (!await _users.IsInRoleAsync(user, "student"))
            await _users.AddToRoleAsync(user, "student");
        if (await _users.IsInRoleAsync(user, "teacher"))
            await _users.RemoveFromRoleAsync(user, "teacher");

        var profile = await _db.Profiles
            .Include(x => x.Student)
            .Include(x => x.Teacher)
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (profile == null) return NotFound();

        profile.Teacher = null;
        profile.Student ??= new StudentInfo();
        await _db.SaveChangesAsync();

        TempData["ok"] = "Đã chuyển về học sinh.";
        return RedirectToAction(nameof(Index));
    }
}
