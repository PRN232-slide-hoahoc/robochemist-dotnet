using System;
using System.Collections.Generic;

namespace RoboChemist.ExamService.Model.Models;

public partial class Question
{
    public Guid QuestionId { get; set; }

    public Guid TopicId { get; set; }  // NOT NULL - required field

    public string QuestionType { get; set; } = null!;

    public string QuestionText { get; set; } = null!;

    public string? Explanation { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Examquestion> Examquestions { get; set; } = new List<Examquestion>();

    public virtual ICollection<Option> Options { get; set; } = new List<Option>();
}
