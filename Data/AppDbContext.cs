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
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<TeacherSubject> TeacherSubjects => Set<TeacherSubject>();
    public DbSet<Problem> Problems => Set<Problem>();
    public DbSet<ProblemChoice> ProblemChoices => Set<ProblemChoice>();
    public DbSet<Contest> Contests => Set<Contest>();
    public DbSet<ContestProblem> ContestProblems => Set<ContestProblem>();
    public DbSet<ContestAttempt> ContestAttempts => Set<ContestAttempt>();
    public DbSet<ContestAnswer> ContestAnswers => Set<ContestAnswer>();

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

        mb.Entity<Subject>(e =>
        {
            e.HasIndex(x => x.Code).IsUnique(); // Mã môn duy nhất
        });
        mb.Entity<TeacherSubject>(e =>
        {
            e.HasIndex(x => new { x.SubjectId, x.TeacherProfileId }).IsUnique(); // tránh trùng phân công
            e.HasOne(ts => ts.Subject)
             .WithMany(s => s.Teachers)
             .HasForeignKey(ts => ts.SubjectId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(ts => ts.TeacherProfile)
             .WithMany() // nếu muốn xem từ phía giáo viên, có thể thêm ICollection<TeacherSubject> vào Profile
             .HasForeignKey(ts => ts.TeacherProfileId)
             .OnDelete(DeleteBehavior.Restrict);
        });
        // ===== Problem / ProblemChoice =====
        mb.Entity<Problem>(e =>
        {
            e.HasOne(x => x.Subject)
             .WithMany()
             .HasForeignKey(x => x.SubjectId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(x => x.AuthorProfile)
             .WithMany()
             .HasForeignKey(x => x.AuthorProfileId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasIndex(x => x.SubjectId);
        });

        mb.Entity<ProblemChoice>(e =>
        {
            e.HasOne(c => c.Problem)
             .WithMany(p => p.Choices)
             .HasForeignKey(c => c.ProblemId)
             .OnDelete(DeleteBehavior.Cascade);

            // Tuỳ chọn: đảm bảo thứ tự duy nhất
            e.HasIndex(c => new { c.ProblemId, c.Order }).IsUnique();
        });
        // ===== Contest / ContestProblem / ContestAttempt / ContestAnswer =====
        mb.Entity<Contest>(e =>
        {
            e.HasOne(c => c.Subject)
             .WithMany()
             .HasForeignKey(c => c.SubjectId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(c => c.AuthorProfile)
             .WithMany()
             .HasForeignKey(c => c.AuthorProfileId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasIndex(c => new { c.SubjectId, c.IsPublished, c.StartAt });
        });

        mb.Entity<ContestProblem>(e =>
        {
            e.Property(x => x.PointsOverride).HasColumnType("decimal(9,2)");

            e.HasOne(cp => cp.Contest)
             .WithMany(c => c.Problems)
             .HasForeignKey(cp => cp.ContestId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(cp => cp.Problem)
             .WithMany()
             .HasForeignKey(cp => cp.ProblemId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(cp => new { cp.ContestId, cp.ProblemId }).IsUnique();
            e.HasIndex(cp => new { cp.ContestId, cp.Order }).IsUnique();
        });

        mb.Entity<ContestAttempt>(e =>
        {
            e.Property(x => x.Score).HasColumnType("decimal(9,2)");

            e.HasOne(a => a.Contest)
             .WithMany(c => c.Attempts)
             .HasForeignKey(a => a.ContestId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(a => a.StudentProfile)
             .WithMany()
             .HasForeignKey(a => a.StudentProfileId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(a => new { a.ContestId, a.StudentProfileId });
        });

        mb.Entity<ContestAnswer>(e =>
        {
            e.HasOne(sa => sa.Attempt)
             .WithMany(a => a.Answers)
             .HasForeignKey(sa => sa.AttemptId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(sa => sa.Problem)
             .WithMany()
             .HasForeignKey(sa => sa.ProblemId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(sa => sa.SelectedChoice)
             .WithMany()
             .HasForeignKey(sa => sa.SelectedChoiceId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(sa => new { sa.AttemptId, sa.ProblemId }).IsUnique();
        });
    }
}
