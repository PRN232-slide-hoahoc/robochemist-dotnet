using System;
using System.Collections.Generic;

namespace RoboChemist.ExamService.Model.Models;

public partial class Matrix
{
    public Guid MatrixId { get; set; }

    public string Name { get; set; } = null!;

    public int? TotalQuestion { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Examrequest> Examrequests { get; set; } = new List<Examrequest>();

    public virtual ICollection<Matrixdetail> Matrixdetails { get; set; } = new List<Matrixdetail>();
}
