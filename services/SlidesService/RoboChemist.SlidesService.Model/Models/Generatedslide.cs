using System;
using System.Collections.Generic;

namespace RoboChemist.SlidesService.Model.Models;

public partial class Generatedslide
{
    public int Id { get; set; }

    public int? SlideRequestId { get; set; }

    public string? JsonContent { get; set; }

    public string? FileFormat { get; set; }

    public string? FilePath { get; set; }

    public string? FileName { get; set; }

    public long? FileSize { get; set; }

    public int? SlideCount { get; set; }

    public string? Metadata { get; set; }

    public string? GenerationStatus { get; set; }

    public double? ProcessingTime { get; set; }

    public DateTime? GeneratedAt { get; set; }

    public virtual Sliderequest? SlideRequest { get; set; }
}
