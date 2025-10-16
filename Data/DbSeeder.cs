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
        var admin = await userMgr.FindByNameAsync("admin");
        if (admin == null)
        {
            admin = new IdentityUser { UserName = "admin", Email = "admin@example.com", EmailConfirmed = true };
            await userMgr.CreateAsync(admin, "Admin@123");
            await userMgr.AddToRoleAsync(admin, "admin");
        }

        // Đảm bảo Profile cho admin (không cần Teacher/StudentInfo)
        await EnsureProfileAsync(db, admin.Id, fullName: "System Admin");
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
}
