using System.ComponentModel.DataAnnotations;

namespace HeThongQuanLyThi.Models;

/// 
/// Bài tập trắc nghiệm với 4 đáp án nhưng KHÔNG dùng nhãn cố định (A/B/C/D).
/// Dùng thuộc tính Order để hiển thị và có thể trộn thứ tự khi render.
/// 
public class Problem
{
    public int Id { get; set; }

    /// Câu hỏi (văn bản/markdown ngắn)
    [Required, MaxLength(2000)]
    public string Content { get; set; } = default!;

    /// Giải thích chung cho câu hỏi (tuỳ chọn)
    [MaxLength(2000)]
    public string? Explanation { get; set; }


    // (Tuỳ chọn) Liên kết môn học & tác giả
    public int? SubjectId { get; set; }
    public Subject? Subject { get; set; }

    public int? AuthorProfileId { get; set; }
    public Profile? AuthorProfile { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// Danh sách đáp án (kỳ vọng 4 đáp án). Không dùng nhãn A/B/C/D để dễ đảo thứ tự.
    public ICollection<ProblemChoice> Choices { get; set; } = new List<ProblemChoice>();
}

/// 
/// Đáp án cho một Problem, không có nhãn cố định. Dùng Order để sắp xếp/đảo.
/// 
public class ProblemChoice
{
    public int Id { get; set; }

    public int ProblemId { get; set; }
    public Problem Problem { get; set; } = default!;

    /// Nội dung đáp án
    [Required, MaxLength(1000)]
    public string Text { get; set; } = default!;

    /// Giải thích riêng cho đáp án (tuỳ chọn)
    [MaxLength(2000)]
    public string? Explanation { get; set; }

    /// Đáp án đúng?
    public bool IsCorrect { get; set; } = false;

    /// Thứ tự hiển thị (0..n). Có thể random khi render.
    public int Order { get; set; } = 0;
}
