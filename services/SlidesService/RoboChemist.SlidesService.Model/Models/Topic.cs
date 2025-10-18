using System;
using System.Collections.Generic;

namespace RoboChemist.SlidesService.Model.Models;

public partial class Topic
{
    public Guid Id { get; set; }

    public string TopicName { get; set; } = null!;

    public string? Description { get; set; }

    public Guid GradeId { get; set; }

    public virtual Grade Grade { get; set; } = null!;

    public virtual ICollection<Syllabus> Syllabi { get; set; } = new List<Syllabus>();
}
