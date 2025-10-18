using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HeThongQuanLyThi.Models;

namespace HeThongQuanLyThi.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Roles
        foreach (var r in new[] { "admin", "teacher", "student" })
            if (!await roleMgr.RoleExistsAsync(r))
                await roleMgr.CreateAsync(new IdentityRole(r));

        // Admin
        var admins = new[]  // Danh sách admin khởi tạo
        {
            new { UserName = "admin", Email = "admin@example.com", Password = "Admin@123" }
        };
        foreach (var a in admins)
        {
            var admin = await userMgr.FindByNameAsync(a.UserName);
            if (admin == null)
            {
                admin = new IdentityUser { UserName = a.UserName, Email = a.Email, EmailConfirmed = true };
                await userMgr.CreateAsync(admin, a.Password);
                await userMgr.AddToRoleAsync(admin, "admin");
            }

            // Đảm bảo Profile cho admin (không cần Teacher/StudentInfo)
            await EnsureProfileAsync(db, admin.Id, fullName: "System Admin");
        }

        await EnsureDefaultSubjectsAsync(db);
        await db.SaveChangesAsync();
    }

    public static async Task<Profile> EnsureProfileAsync(AppDbContext db, string userId, string? fullName = null)
    {
        var p = await db.Profiles.FirstOrDefaultAsync(x => x.UserId == userId);
        if (p != null) return p;
        p = new Profile { UserId = userId, FullName = fullName ?? "" };
        db.Profiles.Add(p);
        return p;
    }

    private static async Task EnsureDefaultSubjectsAsync(AppDbContext db)
    {
        var defaults = new List<Subject>
        {
            new() { Code = "MATH",  Name = "Toán học" },
            new() { Code = "LIT",   Name = "Ngữ văn" },
            new() { Code = "ENG",   Name = "Tiếng Anh" },
            new() { Code = "PHYS",  Name = "Vật lý" },
            new() { Code = "CHEM",  Name = "Hóa học" },
            new() { Code = "BIO",   Name = "Sinh học" },
            new() { Code = "HIST",  Name = "Lịch sử" },
            new() { Code = "GEO",   Name = "Địa lý" },
            new() { Code = "CIV",   Name = "Giáo dục công dân" },
            new() { Code = "IT",    Name = "Tin học" },
            new() { Code = "TECH",  Name = "Công nghệ" },
            new() { Code = "PE",    Name = "Thể dục" },
            new() { Code = "ART",   Name = "Mỹ thuật" },
            new() { Code = "MUSIC", Name = "Âm nhạc" }
        };

        // Lấy các code đã có để tránh thêm trùng
        var existedCodes = await db.Subjects
            .Select(s => s.Code)
            .ToListAsync();

        var existed = new HashSet<string>(existedCodes, StringComparer.OrdinalIgnoreCase);
        var toAdd = defaults.Where(s => !existed.Contains(s.Code)).ToList();

        if (toAdd.Count > 0)
        {
            await db.Subjects.AddRangeAsync(toAdd);
            await db.SaveChangesAsync();
        }
    }
}
