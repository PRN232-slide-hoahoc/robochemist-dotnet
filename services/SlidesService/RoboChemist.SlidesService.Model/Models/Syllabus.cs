using System;
using System.Collections.Generic;

namespace RoboChemist.SlidesService.Model.Models;

public partial class Syllabus
{
    public Guid Id { get; set; }

    public Guid TopicId { get; set; }

    public string Lesson { get; set; } = null!;

    public string? LearningObjectives { get; set; }

    public string? ContentOutline { get; set; }

    public string? KeyConcepts { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? LessonOrder { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Sliderequest> Sliderequests { get; set; } = new List<Sliderequest>();

    public virtual Topic Topic { get; set; } = null!;
}
