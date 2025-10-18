using HeThongQuanLyThi.Data;
using HeThongQuanLyThi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongQuanLyThi.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "admin")]
public class SubjectsController : Controller
{
    private readonly AppDbContext _db;

    public SubjectsController(AppDbContext db) => _db = db;

    // GET: /Admin/Subjects
    public async Task<IActionResult> Index(string? q = null)
    {
        var query = _db.Subjects.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            var key = q.Trim();
            query = query.Where(s =>
                EF.Functions.Like(s.Name, $"%{key}%") ||
                EF.Functions.Like(s.Code, $"%{key}%") ||
                EF.Functions.Like(s.GradeLevel!, $"%{key}%"));
        }

        var items = await query
            .OrderBy(s => s.GradeLevel)
            .ThenBy(s => s.Name)
            .Take(1000)
            .ToListAsync();

        ViewBag.Query = q;
        return View(items);
    }

    // GET: /Admin/Subjects/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var subject = await _db.Subjects.FirstOrDefaultAsync(s => s.Id == id);
        if (subject == null) return NotFound();
        return View(subject);
    }

    // GET: /Admin/Subjects/Create
    public IActionResult Create() => View(new Subject());

    // POST: /Admin/Subjects/Create
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Subject model)
    {
        if (!ModelState.IsValid) return View(model);

        var dup = await _db.Subjects.AnyAsync(s => s.Code.ToLower() == model.Code.ToLower());
        if (dup)
        {
            ModelState.AddModelError(nameof(Subject.Code), "Mã môn đã tồn tại.");
            return View(model);
        }

        _db.Subjects.Add(model);
        await _db.SaveChangesAsync();
        TempData["ok"] = "Đã tạo môn học.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Admin/Subjects/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var subject = await _db.Subjects.FindAsync(id);
        if (subject == null) return NotFound();
        return View(subject);
    }

    // POST: /Admin/Subjects/Edit/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Subject model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);

        var subject = await _db.Subjects.FindAsync(id);
        if (subject == null) return NotFound();

        var dup = await _db.Subjects.AnyAsync(s => s.Id != id && s.Code.ToLower() == model.Code.ToLower());
        if (dup)
        {
            ModelState.AddModelError(nameof(Subject.Code), "Mã môn đã tồn tại.");
            return View(model);
        }

        subject.Code        = model.Code.Trim();
        subject.Name        = model.Name.Trim();
        subject.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
        subject.GradeLevel  = string.IsNullOrWhiteSpace(model.GradeLevel) ? null : model.GradeLevel.Trim();
        subject.Credits     = model.Credits;
        subject.IsActive    = model.IsActive;

        await _db.SaveChangesAsync();
        TempData["ok"] = "Đã cập nhật môn học.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Admin/Subjects/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var subject = await _db.Subjects.FindAsync(id);
        if (subject == null) return NotFound();
        return View(subject);
    }

    // POST: /Admin/Subjects/Delete/5
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var subject = await _db.Subjects.FindAsync(id);
        if (subject == null) return NotFound();

        _db.Subjects.Remove(subject);
        await _db.SaveChangesAsync();
        TempData["ok"] = "Đã xoá môn học.";
        return RedirectToAction(nameof(Index));
    }
}