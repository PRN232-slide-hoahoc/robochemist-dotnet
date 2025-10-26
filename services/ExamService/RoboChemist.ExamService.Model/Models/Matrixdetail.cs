using System;
using System.Collections.Generic;

namespace RoboChemist.ExamService.Model.Models;

public partial class Matrixdetail
{
    public Guid MatrixDetailsId { get; set; }

    public Guid MatrixId { get; set; }

    public int TopicId { get; set; }

    public string QuestionType { get; set; } = null!;

    public int QuestionCount { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public virtual Matrix Matrix { get; set; } = null!;
}
