namespace RoboChemist.Shared.DTOs.SlideDTOs
{
    public class SlideRequestDTOs
    {
        public class GenerateSlideRequest
        {
            public Guid SyllabusId { get; set; }

            public int? NumberOfSlides { get; set; }

            public string? AiPrompt { get; set; }

            public Guid? TemplateId { get; set; }
        }

        public class DataForGenerateSlideRequest
        {
            public string GradeName { get; set; } = null!;

            public int? TopicOrder { get; set; }

            public string TopicName { get; set; } = null!;

            public int? LessonOrder { get; set; }

            public string Lesson { get; set; } = null!;

            public string? LearningObjectives { get; set; }

            public string? ContentOutline { get; set; }

            public string? KeyConcepts { get; set; }

            public int? NumberOfSlides { get; set; }

            public string? AiPrompt { get; set; }
        }
    }
}
