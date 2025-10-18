using System;
using System.Collections.Generic;

namespace RoboChemist.ExamService.Model.Models;

public partial class Examquestion
{
    public Guid ExamQuestionId { get; set; }

    public Guid UserId { get; set; }

    public Guid? MatrixId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual Matrix? Matrix { get; set; }
}
