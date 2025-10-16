using System.ComponentModel.DataAnnotations;

namespace HeThongQuanLyThi.Models;

public class StudentProfileEditVm
{
    [Required, StringLength(150)]
    public string FullName { get; set; } = default!;

    public DateTime? DateOfBirth { get; set; }

    [StringLength(50)]
    public string? Gender { get; set; }

    [StringLength(200)]
    public string? Address { get; set; }

    [StringLength(30)]
    public string? Phone { get; set; }

    // Thông tin học sinh
    [StringLength(20)]
    public string? GradeLevel { get; set; }   // ví dụ: "10","11","12"

    [StringLength(50)]
    public string? ClassName { get; set; }    // ví dụ: "12A1"

    public int? EnrollmentYear { get; set; }

    [StringLength(30)]
    public string? ParentPhone { get; set; }
}
