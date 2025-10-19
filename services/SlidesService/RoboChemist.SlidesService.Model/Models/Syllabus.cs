using System;
using System.Collections.Generic;

namespace RoboChemist.SlidesService.Model.Models;

public partial class Syllabus
{
    public Guid Id { get; set; }

    public string Subject { get; set; } = null!;

    public Guid TopicId { get; set; }

    public string Lesson { get; set; } = null!;

    public string? LearningObjectives { get; set; }

    public string? ContentOutline { get; set; }

    public string? KeyConcepts { get; set; }

    public string? TeachingNotes { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Sliderequest> Sliderequests { get; set; } = new List<Sliderequest>();

    public virtual Topic Topic { get; set; } = null!;
}
