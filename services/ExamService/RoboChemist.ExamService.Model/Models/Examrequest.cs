using System;
using System.Collections.Generic;

namespace RoboChemist.ExamService.Model.Models;

public partial class Examrequest
{
    public Guid ExamRequestId { get; set; }

    public Guid UserId { get; set; }

    public Guid? MatrixId { get; set; }

    public int GradeId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<Generatedexam> Generatedexams { get; set; } = new List<Generatedexam>();

    public virtual Matrix? Matrix { get; set; }
}
