using System;
using System.Collections.Generic;

namespace RoboChemist.SlidesService.Model.Models;

public partial class Sliderequest
{
    public int Id { get; set; }

    public int? SyllabusId { get; set; }

    public int? UserId { get; set; }

    public string? RequestType { get; set; }

    public string? UserRequirements { get; set; }

    public int? NumberOfSlides { get; set; }

    public string? TemplateStyle { get; set; }

    public string? AiPrompt { get; set; }

    public string? Status { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime? RequestedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public virtual ICollection<Generatedslide> Generatedslides { get; set; } = new List<Generatedslide>();

    public virtual Syllabus? Syllabus { get; set; }
}
