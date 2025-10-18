using HeThongQuanLyThi.Data;
using HeThongQuanLyThi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongQuanLyThi.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "admin")]
public class ContestsController(AppDbContext db) : Controller
{
    private readonly AppDbContext _db = db;

    // GET: /Admin/Contests
    public async Task<IActionResult> Index(string? q = null, int? subjectId = null)
    {
        var query = _db.Contests.Include(c => c.Subject).AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var key = q.Trim();
            query = query.Where(c =>
                EF.Functions.Like(c.Title, $"%{key}%") ||
                EF.Functions.Like(c.Description!, $"%{key}%"));
        }

        if (subjectId is not null)
            query = query.Where(c => c.SubjectId == subjectId);

        var items = await query
            .OrderByDescending(c => c.StartAt)
            .Take(500)
            .ToListAsync();

        ViewBag.Subjects = await _db.Subjects.OrderBy(s => s.Name).ToListAsync();
        ViewBag.Query = q;
        ViewBag.SubjectId = subjectId;

        return View(items);
    }

    // GET: /Admin/Contests/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var c = await _db.Contests
            .Include(x => x.Subject)
            .Include(x => x.Problems).ThenInclude(cp => cp.Problem)
            .Include(x => x.Attempts)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (c == null) return NotFound();
        return View(c);
    }

    // GET: /Admin/Contests/Create
    public async Task<IActionResult> Create()
    {
        var subjects = await _db.Subjects.OrderBy(s => s.Name).ToListAsync();
        ViewBag.Subjects = subjects;

        var vm = new ContestEditVm();
        if (subjects.Count > 0)
        {
            vm.SubjectId = subjects.First().Id;
            vm.Items = await BuildItemsForSubjectAsync(vm.SubjectId, null);
        }
        return View(vm);
    }

    // POST: /Admin/Contests/Create
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ContestEditVm vm, string? reload)
    {
        ViewBag.Subjects = await _db.Subjects.OrderBy(s => s.Name).ToListAsync();

        // Người dùng bấm nút "Tải DS câu hỏi" (reload list theo Subject)
        if (!string.IsNullOrEmpty(reload))
        {
            vm.Items = await BuildItemsForSubjectAsync(vm.SubjectId, null);
            return View(vm);
        }

        await ValidateContestAsync(vm);
        if (!ModelState.IsValid)
        {
            if (vm.Items == null || vm.Items.Count == 0)
                vm.Items = await BuildItemsForSubjectAsync(vm.SubjectId, null);
            return View(vm);
        }

        var c = new Contest
        {
            Title = vm.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(vm.Description) ? null : vm.Description.Trim(),
            SubjectId = vm.SubjectId,
            StartAt = vm.StartAt,
            EndAt = vm.EndAt,
            TimeLimitMinutes = vm.TimeLimitMinutes,
            MaxAttemptsPerStudent = vm.MaxAttemptsPerStudent,
            IsPublished = vm.IsPublished,
            ShuffleQuestions = vm.ShuffleQuestions,
            ShuffleChoices = vm.ShuffleChoices
        };

        // Gắn danh sách câu hỏi đã chọn (inline)
        var selected = (vm.Items ?? new()).Where(i => i.Selected).ToList();
        for (int i = 0; i < selected.Count; i++)
            if (selected[i].Order < 0) selected[i].Order = i;

        foreach (var it in selected.OrderBy(i => i.Order).ThenBy(i => i.ProblemId))
        {
            c.Problems.Add(new ContestProblem
            {
                ProblemId = it.ProblemId,
                Order = it.Order,
                PointsOverride = it.PointsOverride
            });
        }

        _db.Contests.Add(c);
        await _db.SaveChangesAsync();
        TempData["ok"] = "Đã tạo cuộc thi.";
        return RedirectToAction(nameof(Details), new { id = c.Id });
    }

    // GET: /Admin/Contests/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var c = await _db.Contests.FindAsync(id);
        if (c == null) return NotFound();

        var vm = new ContestEditVm
        {
            Id = c.Id,
            Title = c.Title,
            Description = c.Description,
            SubjectId = c.SubjectId,
            StartAt = c.StartAt,
            EndAt = c.EndAt,
            TimeLimitMinutes = c.TimeLimitMinutes,
            MaxAttemptsPerStudent = c.MaxAttemptsPerStudent,
            IsPublished = c.IsPublished,
            ShuffleQuestions = c.ShuffleQuestions,
            ShuffleChoices = c.ShuffleChoices,
            Items = await BuildItemsForSubjectAsync(c.SubjectId, c.Id)
        };

        ViewBag.Subjects = await _db.Subjects.OrderBy(s => s.Name).ToListAsync();
        return View(vm);
    }

    // POST: /Admin/Contests/Edit/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ContestEditVm vm, string? reload)
    {
        ViewBag.Subjects = await _db.Subjects.OrderBy(s => s.Name).ToListAsync();

        if (id != vm.Id) return BadRequest();

        // Người dùng bấm nút "Tải DS câu hỏi" để đổi Subject và reload list
        if (!string.IsNullOrEmpty(reload))
        {
            vm.Items = await BuildItemsForSubjectAsync(vm.SubjectId, vm.Id);
            return View(vm);
        }

        await ValidateContestAsync(vm);
        if (!ModelState.IsValid)
        {
            vm.Items ??= await BuildItemsForSubjectAsync(vm.SubjectId, vm.Id);
            return View(vm);
        }

        var c = await _db.Contests.FindAsync(id);
        if (c == null) return NotFound();

        // Cập nhật thông tin chung
        c.Title = vm.Title.Trim();
        c.Description = string.IsNullOrWhiteSpace(vm.Description) ? null : vm.Description.Trim();
        c.SubjectId = vm.SubjectId;
        c.StartAt = vm.StartAt;
        c.EndAt = vm.EndAt;
        c.TimeLimitMinutes = vm.TimeLimitMinutes;
        c.MaxAttemptsPerStudent = vm.MaxAttemptsPerStudent;
        c.IsPublished = vm.IsPublished;
        c.ShuffleQuestions = vm.ShuffleQuestions;
        c.ShuffleChoices = vm.ShuffleChoices;

        // Thay danh sách câu hỏi
        var olds = await _db.ContestProblems.Where(x => x.ContestId == id).ToListAsync();
        _db.ContestProblems.RemoveRange(olds);

        var selected = (vm.Items ?? new()).Where(i => i.Selected).ToList();
        for (int i = 0; i < selected.Count; i++)
            if (selected[i].Order < 0) selected[i].Order = i;

        foreach (var it in selected.OrderBy(i => i.Order).ThenBy(i => i.ProblemId))
        {
            _db.ContestProblems.Add(new ContestProblem
            {
                ContestId = id,
                ProblemId = it.ProblemId,
                Order = it.Order,
                PointsOverride = it.PointsOverride
            });
        }

        await _db.SaveChangesAsync();
        TempData["ok"] = "Đã cập nhật cuộc thi.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // GET: /Admin/Contests/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var c = await _db.Contests.Include(x => x.Subject).FirstOrDefaultAsync(x => x.Id == id);
        if (c == null) return NotFound();
        return View(c);
    }

    // POST: /Admin/Contests/Delete/5
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var c = await _db.Contests.FindAsync(id);
        if (c == null) return NotFound();

        _db.Contests.Remove(c);
        await _db.SaveChangesAsync();
        TempData["ok"] = "Đã xoá cuộc thi.";
        return RedirectToAction(nameof(Index));
    }

    // ===== helpers =====
    private Task ValidateContestAsync(ContestEditVm vm)
    {
        if (vm.EndAt != null && vm.EndAt <= vm.StartAt)
            ModelState.AddModelError(nameof(vm.EndAt), "Thời gian kết thúc phải sau thời gian bắt đầu.");

        if (vm.TimeLimitMinutes < 0)
            ModelState.AddModelError(nameof(vm.TimeLimitMinutes), "Giới hạn thời gian không hợp lệ.");

        if (vm.MaxAttemptsPerStudent < 0)
            ModelState.AddModelError(nameof(vm.MaxAttemptsPerStudent), "Số lượt làm tối đa không hợp lệ.");

        return Task.CompletedTask;
    }

    private async Task<List<ContestProblemItemVm>> BuildItemsForSubjectAsync(int subjectId, int? contestId)
    {
        var allProblems = await _db.Problems
            .Where(p => p.SubjectId == subjectId)
            .OrderBy(p => p.Id)
            .Select(p => new { p.Id, p.Content })
            .ToListAsync();

        Dictionary<int, ContestProblem>? existing = null;
        if (contestId.HasValue)
        {
            existing = await _db.ContestProblems
                .Where(cp => cp.ContestId == contestId.Value)
                .ToDictionaryAsync(cp => cp.ProblemId, cp => cp);
        }

        var items = new List<ContestProblemItemVm>(allProblems.Count);
        foreach (var p in allProblems)
        {
            ContestProblem? ex = null;
            if (existing != null) existing.TryGetValue(p.Id, out ex);

            items.Add(new ContestProblemItemVm
            {
                ProblemId = p.Id,
                Content = p.Content,
                Selected = ex != null,
                Order = ex?.Order ?? 0,
                PointsOverride = ex?.PointsOverride
            });
        }
        return items;
    }
}
