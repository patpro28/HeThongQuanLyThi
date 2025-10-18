using System.ComponentModel.DataAnnotations;

namespace HeThongQuanLyThi.Models;

public class Subject
{
    public int Id { get; set; }

    [Required, MaxLength(20)]
    public string Code { get; set; } = default!;          // Mã môn: VD MATH10

    [Required, MaxLength(150)]
    public string Name { get; set; } = default!;          // Tên môn: Toán 10

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(20)]
    public string? GradeLevel { get; set; }               // "10","11","12"...

    public int? Credits { get; set; }                     // Số tín chỉ (nếu dùng)
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Nhiều giáo viên có thể dạy 1 môn (phân công)
    public ICollection<TeacherSubject> Teachers { get; set; } = new List<TeacherSubject>();
}

public class TeacherSubject
{
    public int Id { get; set; }

    // FK -> Subject
    public int SubjectId { get; set; }
    public Subject Subject { get; set; } = default!;

    // FK -> Profile (giáo viên). Dùng ProfileId vì TeacherInfo gắn 1-1 với Profile.
    public int TeacherProfileId { get; set; }
    public Profile TeacherProfile { get; set; } = default!;

    public bool IsPrimary { get; set; } = false;          // giáo viên chính/phụ (tuỳ nhu cầu)
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}
