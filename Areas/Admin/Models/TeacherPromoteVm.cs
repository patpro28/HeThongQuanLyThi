using System.ComponentModel.DataAnnotations;

namespace HeThongQuanLyThi.Models;

public class TeacherPromoteVm
{
    [Required]
    public string UserId { get; set; } = default!;
    public string? FullName { get; set; }
    public bool HasStudent { get; set; }
    public bool HasTeacher { get; set; }
}

public class PromoteTeacherVm
{
    [Required]
    public string UserId { get; set; } = default!;
    public string? FullName { get; set; }
    public string? Subject { get; set; }
    public string? Degree { get; set; }
    public string? HomeroomClass { get; set; }
}
