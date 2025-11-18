using System;
using System.Collections.Generic;

namespace RoboChemist.ExamService.Model.Models;

public partial class Matrixdetail
{
    public Guid MatrixDetailsId { get; set; }

    public Guid MatrixId { get; set; }

    public Guid? TopicId { get; set; }

    public string QuestionType { get; set; } = null!;

    public int QuestionCount { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsActive { get; set; }

    public string? Level { get; set; }

    public virtual Matrix Matrix { get; set; } = null!;
}
