using System.ComponentModel.DataAnnotations;

namespace RoboChemist.Shared.DTOs.QuestionDTOs
{
    public class QuestionDTOs
    {
        /// <summary>
        /// DTO for returning question details
        /// </summary>
        public class QuestionDto
        {
            /// <summary>
            /// Unique identifier of the question
            /// </summary>
            public Guid Id { get; set; }

            /// <summary>
            /// Topic ID that this question belongs to (from Slide Service)
            /// </summary>
            public Guid TopicId { get; set; }

            /// <summary>
            /// Name of the topic (populated from Slide Service)
            /// </summary>
            public string TopicName { get; set; } = string.Empty;

            /// <summary>
            /// Type of question (Multiple Choice, True/False, etc.)
            /// </summary>
            public string QuestionType { get; set; } = string.Empty;

            /// <summary>
            /// The question text/content
            /// </summary>
            public string QuestionText { get; set; } = string.Empty;

            /// <summary>
            /// Optional explanation for the correct answer
            /// </summary>
            public string? Explanation { get; set; }

            /// <summary>
            /// List of answer options with their details
            /// </summary>
            public List<OptionDto> Options { get; set; } = new();

            /// <summary>
            /// Indicates if the question is active
            /// </summary>
            public bool IsActive { get; set; }

            /// <summary>
            /// Created date
            /// </summary>
            public DateTime? CreatedAt { get; set; }

            /// <summary>
            /// Last updated date
            /// </summary>
            public DateTime? UpdatedAt { get; set; }
        }

        /// <summary>
        /// DTO for creating a new question
        /// </summary>
        public class CreateQuestionDto
        {
            /// <summary>
            /// Topic ID from Slide Service (required)
            /// </summary>
            [Required(ErrorMessage = "Topic ID là bắt buộc")]
            public Guid TopicId { get; set; }

            /// <summary>
            /// Type of question (required)
            /// </summary>
            [Required(ErrorMessage = "Loại câu hỏi là bắt buộc")]
            [StringLength(50, ErrorMessage = "Loại câu hỏi không được vượt quá 50 ký tự")]
            public string QuestionType { get; set; } = string.Empty;

            /// <summary>
            /// Question text/content (required)
            /// </summary>
            [Required(ErrorMessage = "Nội dung câu hỏi là bắt buộc")]
            [StringLength(2000, MinimumLength = 5, ErrorMessage = "Câu hỏi phải từ 5-2000 ký tự")]
            public string QuestionText { get; set; } = string.Empty;

            /// <summary>
            /// Optional explanation for the answer
            /// </summary>
            [StringLength(1000, ErrorMessage = "Giải thích không được vượt quá 1000 ký tự")]
            public string? Explanation { get; set; }

            /// <summary>
            /// List of answer options (minimum 2 required)
            /// </summary>
            [Required(ErrorMessage = "Phải có ít nhất 2 đáp án")]
            [MinLength(2, ErrorMessage = "Phải có ít nhất 2 đáp án")]
            public List<CreateOptionDto> Options { get; set; } = new();
        }

        /// <summary>
        /// DTO for updating an existing question
        /// </summary>
        public class UpdateQuestionDto
        {
            /// <summary>
            /// Topic ID from Slide Service
            /// </summary>
            [Required(ErrorMessage = "Topic ID là bắt buộc")]
            public Guid TopicId { get; set; }

            /// <summary>
            /// Type of question
            /// </summary>
            [Required(ErrorMessage = "Loại câu hỏi là bắt buộc")]
            [StringLength(50, ErrorMessage = "Loại câu hỏi không được vượt quá 50 ký tự")]
            public string QuestionType { get; set; } = string.Empty;

            /// <summary>
            /// Question text/content
            /// </summary>
            [Required(ErrorMessage = "Nội dung câu hỏi là bắt buộc")]
            [StringLength(2000, MinimumLength = 5, ErrorMessage = "Câu hỏi phải từ 5-2000 ký tự")]
            public string QuestionText { get; set; } = string.Empty;

            /// <summary>
            /// Optional explanation
            /// </summary>
            [StringLength(1000, ErrorMessage = "Giải thích không được vượt quá 1000 ký tự")]
            public string? Explanation { get; set; }

            /// <summary>
            /// List of answer options
            /// </summary>
            [Required(ErrorMessage = "Phải có ít nhất 2 đáp án")]
            [MinLength(2, ErrorMessage = "Phải có ít nhất 2 đáp án")]
            public List<CreateOptionDto> Options { get; set; } = new();

            /// <summary>
            /// Active status
            /// </summary>
            public bool IsActive { get; set; } = true;
        }

        /// <summary>
        /// DTO for answer option details
        /// </summary>
        public class OptionDto
        {
            /// <summary>
            /// Unique identifier of the option
            /// </summary>
            public Guid Id { get; set; }

            /// <summary>
            /// Answer text
            /// </summary>
            public string Answer { get; set; } = string.Empty;

            /// <summary>
            /// Indicates if this is the correct answer
            /// </summary>
            public bool IsCorrect { get; set; }
        }

        /// <summary>
        /// DTO for creating/updating answer options
        /// </summary>
        public class CreateOptionDto
        {
            /// <summary>
            /// Answer text (required)
            /// </summary>
            [Required(ErrorMessage = "Nội dung đáp án là bắt buộc")]
            [StringLength(500, MinimumLength = 1, ErrorMessage = "Đáp án phải từ 1-500 ký tự")]
            public string Answer { get; set; } = string.Empty;

            /// <summary>
            /// Indicates if this is the correct answer
            /// </summary>
            public bool IsCorrect { get; set; }
        }
    }
}
