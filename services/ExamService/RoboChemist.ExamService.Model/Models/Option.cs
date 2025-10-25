using System;
using System.Collections.Generic;

namespace RoboChemist.ExamService.Model.Models;

public partial class Option
{
    public Guid OptionId { get; set; }

    public Guid QuestionId { get; set; }

    public string Answer { get; set; } = null!;

    public bool? IsCorrect { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Question Question { get; set; } = null!;
}
