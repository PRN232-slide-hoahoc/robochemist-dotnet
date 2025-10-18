using System;
using System.Collections.Generic;

namespace RoboChemist.ExamService.Model.Models;

public partial class Generatedexam
{
    public Guid GeneratedExamId { get; set; }

    public Guid ExamRequestId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string Status { get; set; } = null!;

    public Guid QuestId { get; set; }

    public virtual Examrequest ExamRequest { get; set; } = null!;
}
