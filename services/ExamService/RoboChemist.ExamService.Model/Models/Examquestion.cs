using System;
using System.Collections.Generic;

namespace RoboChemist.ExamService.Model.Models;

public partial class Examquestion
{
    public Guid ExamQuestionId { get; set; }

    public Guid GeneratedExamId { get; set; }

    public Guid QuestionId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual Generatedexam GeneratedExam { get; set; } = null!;

    public virtual Question Question { get; set; } = null!;
}
