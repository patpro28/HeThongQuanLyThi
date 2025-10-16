// Models/Profile.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeThongQuanLyThi.Models;

public class Profile
{
    public int Id { get; set; }

    [Required] public string UserId { get; set; } = default!;   // FK -> AspNetUsers.Id
    [MaxLength(150)] public string FullName { get; set; } = "";
    public DateTime? DateOfBirth { get; set; }
    [MaxLength(50)] public string? Gender { get; set; }          // "Nam"/"Nữ"/"Khác"...
    [MaxLength(200)] public string? Address { get; set; }
    [MaxLength(30)] public string? Phone { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // 1-1 optional: chỉ một trong hai tồn tại theo role
    public TeacherInfo? Teacher { get; set; }
    public StudentInfo? Student { get; set; }
}

public class TeacherInfo
{
    public int Id { get; set; }
    public int ProfileId { get; set; }                          // FK -> Profile
    [MaxLength(100)] public string? Subject { get; set; }
    [MaxLength(100)] public string? Degree { get; set; }
    public DateTime? HireDate { get; set; }
    [MaxLength(50)] public string? HomeroomClass { get; set; }
    public Profile Profile { get; set; } = default!;
}

public class StudentInfo
{
    public int Id { get; set; }
    public int ProfileId { get; set; }                          // FK -> Profile
    [MaxLength(20)] public string? GradeLevel { get; set; }     // VD: "10", "11", "12"
    [MaxLength(50)] public string? ClassName { get; set; }      // VD: "12A1"
    public int? EnrollmentYear { get; set; }
    [MaxLength(30)] public string? ParentPhone { get; set; }
    public Profile Profile { get; set; } = default!;
}
