using System.ComponentModel.DataAnnotations;

namespace RoboChemist.Shared.DTOs.SlideDTOs
{
    public class SlideRequestDTOs
    {
        public class GenerateSlideRequest
        {
            public Guid SyllabusId { get; set; }

            [Range(3, 30, ErrorMessage = "Số lượng slide phải nằm trong khoảng từ 3 đến 30.")]
            public int? NumberOfSlides { get; set; }

            [StringLength(300, ErrorMessage = "Hướng dẫn thêm (AiPrompt) không được vượt quá 300 ký tự.")]
            public string? AiPrompt { get; set; }

            public Guid? TemplateId { get; set; }
        }

        public class GetSlidesRequest
        {
            public int PageNumber { get; set; } = 1;

            public int PageSize { get; set; } = 10;

            // Filter parameters
            public Guid? GradeId { get; set; }

            public Guid? TopicId { get; set; }

            public string? GenerationStatus { get; set; }

            // Sort parameters
            public string SortBy { get; set; } = "GeneratedAt";

            public string SortOrder { get; set; } = "desc";
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

        public class ChangeTemplateRequest
        {
            [Required(ErrorMessage = "TemplateId là bắt buộc.")]
            public Guid TemplateId { get; set; }

            [Required(ErrorMessage = "SlideId là bắt buộc.")]
            public Guid SlideId { get; set; }
        }
    }
}
