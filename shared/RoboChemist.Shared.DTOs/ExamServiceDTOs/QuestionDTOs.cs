using System.ComponentModel.DataAnnotations;
using RoboChemist.Shared.Common.Constants;

namespace RoboChemist.Shared.DTOs.ExamServiceDTOs
{
    /// <summary>
    /// DTOs cho Question management
    /// </summary>
    public class QuestionDTOs
    {
        /// <summary>
        /// DTO để tạo mới Question
        /// </summary>
        public class CreateQuestionDto
        {
            /// <summary>
            /// ID của Topic (bắt buộc) - Phải tồn tại và active trong SlidesService
            /// </summary>
            [Required(ErrorMessage = "TopicId là bắt buộc")]
            public Guid TopicId { get; set; }

            /// <summary>
            /// Loại câu hỏi: MultipleChoice, TrueFalse, FillBlank, Essay
            /// </summary>
            [Required(ErrorMessage = "QuestionType là bắt buộc")]
            [RegularExpression("^(MultipleChoice|TrueFalse|FillBlank|Essay)$", 
                ErrorMessage = "QuestionType phải là: MultipleChoice, TrueFalse, FillBlank, hoặc Essay")]
            public string QuestionType { get; set; } = string.Empty;

            /// <summary>
            /// Nội dung câu hỏi
            /// </summary>
            [Required(ErrorMessage = "QuestionText là bắt buộc")]
            [MinLength(10, ErrorMessage = "QuestionText phải có ít nhất 10 ký tự")]
            [MaxLength(1000, ErrorMessage = "QuestionText không được vượt quá 1000 ký tự")]
            public string QuestionText { get; set; } = string.Empty;

            /// <summary>
            /// Giải thích đáp án (optional)
            /// </summary>
            [MaxLength(2000, ErrorMessage = "Explanation không được vượt quá 2000 ký tự")]
            public string? Explanation { get; set; }

            /// <summary>
            /// Danh sách đáp án (tối thiểu 0 cho Essay, 2-6 cho các loại khác)
            /// </summary>
            [Required(ErrorMessage = "Options là bắt buộc")]
            [MaxLength(6, ErrorMessage = "Không được có quá 6 đáp án")]
            public List<CreateOptionDto> Options { get; set; } = new();
        }

        /// <summary>
        /// DTO để tạo nhiều Questions cùng 1 Topic (Bulk Create - For seeding data)
        /// </summary>
        public class BulkCreateQuestionsDto
        {
            /// <summary>
            /// ID của Topic chung cho tất cả câu hỏi
            /// </summary>
            [Required(ErrorMessage = "TopicId là bắt buộc")]
            public Guid TopicId { get; set; }

            /// <summary>
            /// Danh sách câu hỏi cần tạo (không bao gồm TopicId vì dùng chung ở trên)
            /// </summary>
            [Required(ErrorMessage = "Questions là bắt buộc")]
            [MinLength(1, ErrorMessage = "Phải có ít nhất 1 câu hỏi")]
            [MaxLength(100, ErrorMessage = "Tối đa 100 câu hỏi mỗi lần")]
            public List<BulkQuestionItemDto> Questions { get; set; } = new();
        }

        /// <summary>
        /// DTO cho 1 câu hỏi trong bulk create (không có TopicId vì dùng chung)
        /// </summary>
        public class BulkQuestionItemDto
        {
            /// <summary>
            /// Loại câu hỏi: MultipleChoice, TrueFalse, FillBlank, Essay
            /// </summary>
            [Required(ErrorMessage = "QuestionType là bắt buộc")]
            [RegularExpression("^(MultipleChoice|TrueFalse|FillBlank|Essay)$", 
                ErrorMessage = "QuestionType phải là: MultipleChoice, TrueFalse, FillBlank, hoặc Essay")]
            public string QuestionType { get; set; } = string.Empty;

            /// <summary>
            /// Nội dung câu hỏi
            /// </summary>
            [Required(ErrorMessage = "QuestionText là bắt buộc")]
            [MinLength(10, ErrorMessage = "QuestionText phải có ít nhất 10 ký tự")]
            [MaxLength(1000, ErrorMessage = "QuestionText không được vượt quá 1000 ký tự")]
            public string QuestionText { get; set; } = string.Empty;

            /// <summary>
            /// Giải thích đáp án (optional)
            /// </summary>
            [MaxLength(2000, ErrorMessage = "Explanation không được vượt quá 2000 ký tự")]
            public string? Explanation { get; set; }

            /// <summary>
            /// Danh sách đáp án
            /// </summary>
            [Required(ErrorMessage = "Options là bắt buộc")]
            [MaxLength(6, ErrorMessage = "Không được có quá 6 đáp án")]
            public List<CreateOptionDto> Options { get; set; } = new();
        }

        /// <summary>
        /// DTO để tạo Option trong Question
        /// </summary>
        public class CreateOptionDto
        {
            /// <summary>
            /// Nội dung đáp án
            /// </summary>
            [Required(ErrorMessage = "Answer là bắt buộc")]
            [MinLength(1, ErrorMessage = "Answer phải có ít nhất 1 ký tự")]
            [MaxLength(500, ErrorMessage = "Answer không được vượt quá 500 ký tự")]
            public string Answer { get; set; } = string.Empty;

            /// <summary>
            /// Đáp án này có đúng không
            /// </summary>
            [Required(ErrorMessage = "IsCorrect là bắt buộc")]
            public bool IsCorrect { get; set; }
        }

        /// <summary>
        /// DTO để cập nhật Question
        /// </summary>
        public class UpdateQuestionDto
        {
            /// <summary>
            /// ID của Topic (bắt buộc) - Phải tồn tại và active trong SlidesService
            /// </summary>
            [Required(ErrorMessage = "TopicId là bắt buộc")]
            public Guid TopicId { get; set; }

            /// <summary>
            /// Loại câu hỏi: MultipleChoice, TrueFalse, FillBlank, Essay
            /// </summary>
            [Required(ErrorMessage = "QuestionType là bắt buộc")]
            [RegularExpression("^(MultipleChoice|TrueFalse|FillBlank|Essay)$",
                ErrorMessage = "QuestionType phải là: MultipleChoice, TrueFalse, FillBlank, hoặc Essay")]
            public string QuestionType { get; set; } = string.Empty;

            /// <summary>
            /// Nội dung câu hỏi
            /// </summary>
            [Required(ErrorMessage = "QuestionText là bắt buộc")]
            [MinLength(10, ErrorMessage = "QuestionText phải có ít nhất 10 ký tự")]
            [MaxLength(1000, ErrorMessage = "QuestionText không được vượt quá 1000 ký tự")]
            public string QuestionText { get; set; } = string.Empty;

            /// <summary>
            /// Giải thích đáp án (optional)
            /// </summary>
            [MaxLength(2000, ErrorMessage = "Explanation không được vượt quá 2000 ký tự")]
            public string? Explanation { get; set; }

            /// <summary>
            /// Trạng thái: "1" = Active, "0" = Inactive
            /// </summary>
            [Required(ErrorMessage = "Status là bắt buộc")]
            [RegularExpression("^[01]$", ErrorMessage = "Status phải là '0' hoặc '1'")]
            public string Status { get; set; } = RoboChemistConstants.QUESTION_STATUS_ACTIVE;

            /// <summary>
            /// Danh sách đáp án (tối thiểu 0 cho Essay, 2-6 cho các loại khác)
            /// </summary>
            [Required(ErrorMessage = "Options là bắt buộc")]
            [MaxLength(6, ErrorMessage = "Không được có quá 6 đáp án")]
            public List<CreateOptionDto> Options { get; set; } = new();
        }

        /// <summary>
        /// DTO Response cho Question (có enrich TopicName từ SlidesService - API Composition)
        /// </summary>
        public class QuestionResponseDto
        {
            public Guid QuestionId { get; set; }
            public Guid TopicId { get; set; }
            public string? TopicName { get; set; } // [API COMPOSITION] Lấy từ SlidesService
            public string QuestionType { get; set; } = string.Empty;
            public string QuestionText { get; set; } = string.Empty;
            public string? Explanation { get; set; }
            public string Status { get; set; } = string.Empty; // "1" = Active, "0" = Inactive (mapping từ IsActive bool)
            public DateTime? CreatedAt { get; set; }
            public Guid? CreatedBy { get; set; }
            public List<OptionResponseDto> Options { get; set; } = new();
        }

        /// <summary>
        /// DTO Response cho Option
        /// </summary>
        public class OptionResponseDto
        {
            public Guid OptionId { get; set; }
            public string Answer { get; set; } = string.Empty;
            public bool IsCorrect { get; set; } // bool vẫn dùng true/false
            public DateTime? CreatedAt { get; set; }
            public Guid? CreatedBy { get; set; }
        }

        /// <summary>
        /// DTO Response cho Bulk Create
        /// </summary>
        public class BulkCreateQuestionsResponseDto
        {
            public int TotalCreated { get; set; }
            public Guid TopicId { get; set; }
            public string QuestionType { get; set; } = string.Empty;
            public List<Guid> CreatedQuestionIds { get; set; } = new();
            public string Message { get; set; } = string.Empty;
        }
    }
}
