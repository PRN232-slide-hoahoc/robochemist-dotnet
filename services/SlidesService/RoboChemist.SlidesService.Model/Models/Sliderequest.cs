using System;
using System.Collections.Generic;

namespace RoboChemist.SlidesService.Model.Models;

public partial class Sliderequest
{
    public Guid Id { get; set; }

    public Guid SyllabusId { get; set; }

    public Guid UserId { get; set; }

    public int? NumberOfSlides { get; set; }

    public string? AiPrompt { get; set; }

    public string? Status { get; set; }

    public DateTime? RequestedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string? TemplateId { get; set; }

    public virtual ICollection<Generatedslide> Generatedslides { get; set; } = new List<Generatedslide>();

    public virtual Syllabus Syllabus { get; set; } = null!;
}
