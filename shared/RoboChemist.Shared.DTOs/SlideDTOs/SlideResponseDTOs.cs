namespace RoboChemist.Shared.DTOs.SlideDTOs
{
    public class SlideResponseDTOs
    {
        public class SlideDto
        {
            public Guid GeneratedSlideId { get; set; }

            public Guid SlideRequestId { get; set; }

            public string? JsonContent { get; set; }

            public string? FileFormat { get; set; }

            public string? FilePath { get; set; }

            public int? FileSize { get; set; }

            public int? SlideCount { get; set; }

            public string? GenerationStatus { get; set; }

            public double? ProcessingTime { get; set; }

            public DateTime? GeneratedAt { get; set; }
        }

        public class ResponseGenerateDataDto
        {
            public FisrtSlideTemplateDto FirstSlide { get; set; } = null!;

            public TableOfContentSlideTemplateDto TableOfContentSlide { get; set; } = null!;

            public List<ContentSlideTemplateDto> ContentSlides { get; set; } = null!;
        }

        #region slide template dto

        public class FisrtSlideTemplateDto
        {
            public string Title { get; set; } = null!;
            public string Subtitle { get; set; } = null!;
            public string Owner { get; set; } = null!;
        }

        public class TableOfContentSlideTemplateDto
        {
            public List<string> Topics { get; set; } = null!;
        }

        public class ContentSlideTemplateDto
        {
            public string Heading { get; set; } = null!;
            public List<string> BulletPoints { get; set; } = null!;
            public string? ImageDescription { get; set; }
        }

        #endregion

        public class SlideFileInfomationDto
        {
            public string? FileFormat { get; set; }

            public string? FilePath { get; set; }

            public int? FileSize { get; set; }

            public int? SlideCount { get; set; }
        }
    }
}
