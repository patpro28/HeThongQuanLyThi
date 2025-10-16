using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HeThongQuanLyThi.Models;

namespace HeThongQuanLyThi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
        : IdentityDbContext<IdentityUser, IdentityRole, string>(options) // <-- quan trọng
{
    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<TeacherInfo> TeacherInfos => Set<TeacherInfo>();
    public DbSet<StudentInfo> StudentInfos => Set<StudentInfo>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        mb.Entity<Profile>(e =>
        {
            e.HasIndex(x => x.UserId).IsUnique(); // mỗi user chỉ có 1 Profile
            e.HasOne<TeacherInfo>(p => p.Teacher)
             .WithOne(t => t.Profile)
             .HasForeignKey<TeacherInfo>(t => t.ProfileId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne<StudentInfo>(p => p.Student)
             .WithOne(s => s.Profile)
             .HasForeignKey<StudentInfo>(s => s.ProfileId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
