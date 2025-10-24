using System;
using System.Collections.Generic;

namespace RoboChemist.SlidesService.Model.Models;

public partial class Generatedslide
{
    public Guid Id { get; set; }

    public Guid SlideRequestId { get; set; }

    public string? JsonContent { get; set; }

    public string? FileFormat { get; set; }

    public string? FilePath { get; set; }

    public int? FileSize { get; set; }

    public int? SlideCount { get; set; }

    public string? GenerationStatus { get; set; }

    public double? ProcessingTime { get; set; }

    public DateTime? GeneratedAt { get; set; }

    public virtual Sliderequest SlideRequest { get; set; } = null!;
}
