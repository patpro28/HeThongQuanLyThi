using HeThongQuanLyThi.Data;
using HeThongQuanLyThi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongQuanLyThi.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "admin")]
public class ProblemsController(AppDbContext db) : Controller
{
    private readonly AppDbContext _db = db;

    // GET: /Admin/Problems
    public async Task<IActionResult> Index(string? q = null, int? subjectId = null)
    {
        var query = _db.Problems
            .Include(p => p.Subject)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var key = q.Trim();
            query = query.Where(p =>
                EF.Functions.Like(p.Content, $"%{key}%") ||
                EF.Functions.Like(p.Explanation!, $"%{key}%"));
        }

        if (subjectId is not null)
            query = query.Where(p => p.SubjectId == subjectId);

        var items = await query
            .OrderByDescending(p => p.Id)
            .Take(500)
            .ToListAsync();

        ViewBag.Query = q;
        ViewBag.SubjectId = subjectId;
        ViewBag.Subjects = await _db.Subjects.OrderBy(s => s.Name).ToListAsync();

        return View(items);
    }

    // GET: /Admin/Problems/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var p = await _db.Problems
            .Include(x => x.Subject)
            .Include(x => x.Choices.OrderBy(c => c.Order))
            .FirstOrDefaultAsync(x => x.Id == id);

        if (p == null) return NotFound();
        return View(p);
    }

    // GET: /Admin/Problems/Create
    public async Task<IActionResult> Create()
    {
        ViewBag.Subjects = await _db.Subjects.OrderBy(s => s.Name).ToListAsync();
        return View(new ProblemEditVm());
    }

    // POST: /Admin/Problems/Create
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProblemEditVm vm)
    {
        await ValidateChoicesAsync(vm);
        if (!ModelState.IsValid)
        {
            ViewBag.Subjects = await _db.Subjects.OrderBy(s => s.Name).ToListAsync();
            return View(vm);
        }

        var problem = new Problem
        {
            Content = vm.Content.Trim(),
            Explanation = string.IsNullOrWhiteSpace(vm.Explanation) ? null : vm.Explanation.Trim(),
            SubjectId = vm.SubjectId
        };

        NormalizeOrders(vm.Choices);

        foreach (var c in vm.Choices)
        {
            problem.Choices.Add(new ProblemChoice
            {
                Text = c.Text.Trim(),
                Explanation = string.IsNullOrWhiteSpace(c.Explanation) ? null : c.Explanation.Trim(),
                IsCorrect = c.IsCorrect,
                Order = c.Order
            });
        }

        _db.Problems.Add(problem);
        await _db.SaveChangesAsync();
        TempData["ok"] = "Đã tạo bài tập.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Admin/Problems/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var p = await _db.Problems
            .Include(x => x.Choices)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) return NotFound();

        var vm = new ProblemEditVm
        {
            Id = p.Id,
            Content = p.Content,
            Explanation = p.Explanation,
            SubjectId = p.SubjectId,
            Choices = p.Choices
                .OrderBy(c => c.Order)
                .Select(c => new ProblemChoiceVm
                {
                    Id = c.Id,
                    Text = c.Text,
                    Explanation = c.Explanation,
                    IsCorrect = c.IsCorrect,
                    Order = c.Order
                }).ToList()
        };

        // đảm bảo đủ 4 ô
        for (int i = vm.Choices.Count; i < 4; i++)
            vm.Choices.Add(new ProblemChoiceVm { Order = i });

        ViewBag.Subjects = await _db.Subjects.OrderBy(s => s.Name).ToListAsync();
        return View(vm);
    }

    // POST: /Admin/Problems/Edit/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProblemEditVm vm)
    {
        if (vm.Id != id) return BadRequest();

        await ValidateChoicesAsync(vm);
        if (!ModelState.IsValid)
        {
            ViewBag.Subjects = await _db.Subjects.OrderBy(s => s.Name).ToListAsync();
            return View(vm);
        }

        var p = await _db.Problems
            .Include(x => x.Choices)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) return NotFound();

        p.Content = vm.Content.Trim();
        p.Explanation = string.IsNullOrWhiteSpace(vm.Explanation) ? null : vm.Explanation.Trim();
        p.SubjectId = vm.SubjectId;

        // cập nhật choices: thay toàn bộ cho gọn (giữ Order)
        _db.ProblemChoices.RemoveRange(p.Choices);

        NormalizeOrders(vm.Choices);
        foreach (var c in vm.Choices)
        {
            p.Choices.Add(new ProblemChoice
            {
                Text = c.Text.Trim(),
                Explanation = string.IsNullOrWhiteSpace(c.Explanation) ? null : c.Explanation.Trim(),
                IsCorrect = c.IsCorrect,
                Order = c.Order
            });
        }

        await _db.SaveChangesAsync();
        TempData["ok"] = "Đã cập nhật bài tập.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Admin/Problems/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var p = await _db.Problems
            .Include(x => x.Subject)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) return NotFound();
        return View(p);
    }

    // POST: /Admin/Problems/Delete/5
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var p = await _db.Problems.FindAsync(id);
        if (p == null) return NotFound();

        _db.Problems.Remove(p);
        await _db.SaveChangesAsync();
        TempData["ok"] = "Đã xoá bài tập.";
        return RedirectToAction(nameof(Index));
    }

    // ============ helpers ============
    private static void NormalizeOrders(List<ProblemChoiceVm> choices)
    {
        // gán thứ tự 0..n theo vị trí hiện tại
        for (int i = 0; i < choices.Count; i++)
            choices[i].Order = i;
    }

    private Task ValidateChoicesAsync(ProblemEditVm vm)
    {
        // Nếu null: thêm lỗi và dừng, tránh NullReferenceException
        if (vm.Choices == null)
        {
            ModelState.AddModelError("Choices", "Cần đúng 4 đáp án.");
            return Task.CompletedTask;
        }

        // Kiểm tra số lượng phải đúng 4
        if (vm.Choices.Count != 4)
            ModelState.AddModelError("Choices", "Cần đúng 4 đáp án.");

        // Kiểm tra nội dung từng đáp án + đếm số đáp án đúng
        var correctCount = 0;
        for (int i = 0; i < vm.Choices.Count; i++)
        {
            var c = vm.Choices[i];
            if (string.IsNullOrWhiteSpace(c.Text))
                ModelState.AddModelError($"Choices[{i}].Text", "Nội dung đáp án không được để trống.");

            if (c.IsCorrect) correctCount++;
        }

        // Yêu cầu đúng duy nhất 1 đáp án đúng
        if (correctCount == 0)
            ModelState.AddModelError("Choices", "Phải chọn một đáp án đúng.");
        else if (correctCount > 1)
            ModelState.AddModelError("Choices", "Chỉ được một đáp án đúng.");

        return Task.CompletedTask;
    }
}