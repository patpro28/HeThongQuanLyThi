using System.ComponentModel.DataAnnotations;

namespace HeThongQuanLyThi.Models
{
    public class ContestEditVm
    {
        public int? Id { get; set; }

        [Required, MaxLength(150)]
        public string Title { get; set; } = default!;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public int SubjectId { get; set; }

        public DateTime StartAt { get; set; } = DateTime.UtcNow;
        public DateTime? EndAt { get; set; }

        public int TimeLimitMinutes { get; set; } = 0;
        public int MaxAttemptsPerStudent { get; set; } = 1;

        public bool IsPublished { get; set; } = false;
        public bool ShuffleQuestions { get; set; } = true;
        public bool ShuffleChoices { get; set; } = true;

        // INLINE: danh sách câu hỏi thuộc môn được chọn
        public List<ContestProblemItemVm> Items { get; set; } = new();
    }

    public class ContestProblemItemVm
    {
        public int ProblemId { get; set; }
        public string? Content { get; set; }
        public bool Selected { get; set; }
        public int Order { get; set; }
        public decimal? PointsOverride { get; set; }
    }

    // (Giữ lại nếu sau này cần màn riêng; hiện tại CRUD inline không dùng)
    public class ContestManageProblemsVm
    {
        public int ContestId { get; set; }
        public List<ContestProblemItemVm> Items { get; set; } = new();
    }
}