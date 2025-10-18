using System;
using System.Collections.Generic;

namespace RoboChemist.ExamService.Model.Models;

public partial class Matrix
{
    public Guid MatrixId { get; set; }

    public string Name { get; set; } = null!;

    public int TopicId { get; set; }

    public bool? IsDeleted { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsSubmitted { get; set; }

    public virtual ICollection<Examquestion> Examquestions { get; set; } = new List<Examquestion>();

    public virtual ICollection<Examrequest> Examrequests { get; set; } = new List<Examrequest>();

    public virtual ICollection<Matrixdetail> Matrixdetails { get; set; } = new List<Matrixdetail>();
}
