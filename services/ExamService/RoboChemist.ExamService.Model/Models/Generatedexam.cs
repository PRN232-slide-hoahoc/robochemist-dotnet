using System;
using System.Collections.Generic;

namespace RoboChemist.ExamService.Model.Models;

public partial class Generatedexam
{
    public Guid GeneratedExamId { get; set; }

    public Guid ExamRequestId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public string? ExportedFileName { get; set; }

    public DateTime? ExportedAt { get; set; }

    public Guid? ExportedBy { get; set; }

    public string? FileFormat { get; set; }

    public virtual Examrequest ExamRequest { get; set; } = null!;

    public virtual ICollection<Examquestion> Examquestions { get; set; } = new List<Examquestion>();
}
