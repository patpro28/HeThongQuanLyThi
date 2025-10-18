using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HeThongQuanLyThi.Data;
using HeThongQuanLyThi.Models;

namespace HeThongQuanLyThi.Controllers;

[Authorize(Roles = "student")]
[Route("student/profile")]
public class StudentProfileController(AppDbContext db, UserManager<IdentityUser> users) : Controller
{
    private readonly AppDbContext _db = db;
    private readonly UserManager<IdentityUser> _users = users;

    // GET: /student/profile
    [HttpGet("")]
    public async Task<IActionResult> Edit()
    {
        var user = await _users.GetUserAsync(User);
        var p = await _db.Profiles
            .Include(x => x.Student)
            .FirstOrDefaultAsync(x => x.UserId == user!.Id);

        if (p == null) return NotFound("Profile chưa được khởi tạo.");

        var vm = new StudentProfileEditVm
        {
            FullName = p.FullName,
            DateOfBirth = p.DateOfBirth,
            Gender = p.Gender,
            Address = p.Address,
            Phone = p.Phone,
            GradeLevel = p.Student?.GradeLevel,
            ClassName = p.Student?.ClassName,
            EnrollmentYear = p.Student?.EnrollmentYear,
            ParentPhone = p.Student?.ParentPhone
        };
        return View(vm);
    }

    // POST: /student/profile
    [HttpPost("")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(StudentProfileEditVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = await _users.GetUserAsync(User);
        var p = await _db.Profiles
            .Include(x => x.Student)
            .FirstOrDefaultAsync(x => x.UserId == user!.Id);

        if (p == null) return NotFound("Profile chưa được khởi tạo.");

        // cập nhật các trường cho phép
        p.FullName = vm.FullName?.Trim() ?? p.FullName;
        p.DateOfBirth = vm.DateOfBirth;
        p.Gender = string.IsNullOrWhiteSpace(vm.Gender) ? null : vm.Gender.Trim();
        p.Address = string.IsNullOrWhiteSpace(vm.Address) ? null : vm.Address.Trim();
        p.Phone = string.IsNullOrWhiteSpace(vm.Phone) ? null : vm.Phone.Trim();

        p.Student ??= new StudentInfo();
        p.Student.GradeLevel = string.IsNullOrWhiteSpace(vm.GradeLevel) ? null : vm.GradeLevel.Trim();
        p.Student.ClassName = string.IsNullOrWhiteSpace(vm.ClassName) ? null : vm.ClassName.Trim();
        p.Student.EnrollmentYear = vm.EnrollmentYear;
        p.Student.ParentPhone = string.IsNullOrWhiteSpace(vm.ParentPhone) ? null : vm.ParentPhone.Trim();

        await _db.SaveChangesAsync();
        TempData["ok"] = "Đã lưu thay đổi.";
        return RedirectToAction(nameof(Edit));
    }

    // GET: /student/profile/view
    [HttpGet("view")]
    public async Task<IActionResult> Details()
    {
        var user = await _users.GetUserAsync(User);
        var p = await _db.Profiles
            .Include(x => x.Student)
            .FirstOrDefaultAsync(x => x.UserId == user!.Id);

        if (p == null) return NotFound("Profile chưa được khởi tạo.");

        // Trả về view Details với model là Profile (bao gồm StudentInfo)
        return View("Details", p);
    }
}
