

using System.ComponentModel.DataAnnotations;

namespace HeThongQuanLyThi.Models;

/// <summary>
/// Cuộc thi trắc nghiệm theo từng môn, sử dụng bộ câu hỏi Problem/ProblemChoice.
/// </summary>
public class Contest
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Title { get; set; } = default!;

    [MaxLength(1000)]
    public string? Description { get; set; }

    // Bắt buộc thuộc về một môn
    public int SubjectId { get; set; }
    public Subject Subject { get; set; } = default!;

    // Người tạo (giáo viên/admin) - theo Profile
    public int? AuthorProfileId { get; set; }
    public Profile? AuthorProfile { get; set; }

    // Thời gian mở/đóng
    public DateTime StartAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndAt { get; set; }

    // Giới hạn thời gian mỗi lượt làm (0 = không giới hạn)
    public int TimeLimitMinutes { get; set; } = 0;

    // Số lượt làm tối đa / học sinh (0 = không giới hạn)
    public int MaxAttemptsPerStudent { get; set; } = 1;

    public bool IsPublished { get; set; } = false;

    // Tuỳ chọn hiển thị
    public bool ShuffleQuestions { get; set; } = true;
    public bool ShuffleChoices { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ContestProblem> Problems { get; set; } = [];
    public ICollection<ContestAttempt> Attempts { get; set; } = [];
}

/// <summary>
/// Bảng nối giữa Contest và Problem, cho phép sắp xếp và override điểm.
/// </summary>
public class ContestProblem
{
    public int Id { get; set; }

    public int ContestId { get; set; }
    public Contest Contest { get; set; } = default!;

    public int ProblemId { get; set; }
    public Problem Problem { get; set; } = default!;

    // Thứ tự câu hỏi trong cuộc thi
    public int Order { get; set; } = 0;

    // Điểm cho câu (mặc định lấy từ Problem.Points); có thể override
    public decimal? PointsOverride { get; set; }
}

/// <summary>
/// Một lượt làm bài của học sinh trong cuộc thi.
/// </summary>
public class ContestAttempt
{
    public int Id { get; set; }

    public int ContestId { get; set; }
    public Contest Contest { get; set; } = default!;

    public int StudentProfileId { get; set; }
    public Profile StudentProfile { get; set; } = default!;

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SubmittedAt { get; set; }

    // Tổng điểm đạt được (có thể tính lại khi chấm)
    public decimal Score { get; set; } = 0m;

    public ICollection<ContestAnswer> Answers { get; set; } = [];
}

/// <summary>
/// Câu trả lời cho 1 câu hỏi trong một lượt làm bài.
/// </summary>
public class ContestAnswer
{
    public int Id { get; set; }

    public int AttemptId { get; set; }
    public ContestAttempt Attempt { get; set; } = default!;

    public int ProblemId { get; set; }
    public Problem Problem { get; set; } = default!;

    // null = bỏ qua/không chọn
    public int? SelectedChoiceId { get; set; }
    public ProblemChoice? SelectedChoice { get; set; }
}