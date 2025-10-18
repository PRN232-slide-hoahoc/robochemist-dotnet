using System;
using System.Collections.Generic;

namespace RoboChemist.ExamService.Model.Models;

public partial class Question
{
    public Guid QuestionId { get; set; }

    public int TopicId { get; set; }

    public string QuestionType { get; set; } = null!;

    public string Question1 { get; set; } = null!;

    public string? Explanation { get; set; }

    public bool? IsSubmitted { get; set; }

    public virtual ICollection<Option> Options { get; set; } = new List<Option>();
}
