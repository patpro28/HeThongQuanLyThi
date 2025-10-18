using System.ComponentModel.DataAnnotations;

namespace HeThongQuanLyThi.Models;

public class ProblemChoiceVm
{
    public int? Id { get; set; }

    [Required, MaxLength(1000)]
    public string Text { get; set; } = default!;

    [MaxLength(2000)]
    public string? Explanation { get; set; }

    public bool IsCorrect { get; set; }
    public int Order { get; set; }
}

public class ProblemEditVm
{
    public int? Id { get; set; }

    [Required, MaxLength(2000)]
    public string Content { get; set; } = default!;

    [MaxLength(2000)]
    public string? Explanation { get; set; }

    public int? SubjectId { get; set; }   // chọn môn (tuỳ chọn)

    // 4 đáp án
    [MinLength(4), MaxLength(4)]
    public List<ProblemChoiceVm> Choices { get; set; } =
    [
        new ProblemChoiceVm { Order = 0 },
        new ProblemChoiceVm { Order = 1 },
        new ProblemChoiceVm { Order = 2 },
        new ProblemChoiceVm { Order = 3 }
    ];
}