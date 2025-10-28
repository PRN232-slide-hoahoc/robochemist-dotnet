using System.ComponentModel.DataAnnotations;
using RoboChemist.Shared.Common.Constants;

namespace RoboChemist.Shared.DTOs.ExamServiceDTOs
{
    /// <summary>
    /// DTOs cho GeneratedExam (Đề thi được tạo ra)
    /// </summary>
    public class ExamDTOs
    {
        /// <summary>
        /// DTO để yêu cầu tạo đề thi mới
        /// </summary>
        public class CreateExamRequestDto
        {
            /// <summary>
            /// ID của ma trận đề thi
            /// </summary>
            [Required(ErrorMessage = "MatrixId là bắt buộc")]
            public Guid MatrixId { get; set; }

            // GradeId đã bị XÓA - không cần thiết
            // Prompt đã bị XÓA - không cần thiết
        }

        /// <summary>
        /// DTO trả về thông tin yêu cầu tạo đề thi
        /// </summary>
        public class ExamRequestResponseDto
        {
            /// <summary>
            /// ID yêu cầu tạo đề
            /// </summary>
            public Guid ExamRequestId { get; set; }

            /// <summary>
            /// ID người dùng yêu cầu
            /// </summary>
            public Guid UserId { get; set; }

            /// <summary>
            /// ID ma trận đề thi
            /// </summary>
            public Guid MatrixId { get; set; }

            /// <summary>
            /// Tên ma trận đề thi
            /// </summary>
            public string MatrixName { get; set; } = string.Empty;

            /// <summary>
            /// Trạng thái yêu cầu (Pending, Processing, Completed, Failed)
            /// </summary>
            public string Status { get; set; } = string.Empty;

            // GradeId và GradeName đã bị XÓA
            // Prompt đã bị XÓA

            /// <summary>
            /// Ngày tạo yêu cầu
            /// </summary>
            public DateTime? CreatedAt { get; set; }

            /// <summary>
            /// Danh sách đề thi đã được tạo ra từ yêu cầu này
            /// </summary>
            public List<GeneratedExamResponseDto> GeneratedExams { get; set; } = new();
        }

        /// <summary>
        /// DTO trả về thông tin đề thi đã được tạo
        /// </summary>
        public class GeneratedExamResponseDto
        {
            /// <summary>
            /// ID đề thi được tạo
            /// </summary>
            public Guid GeneratedExamId { get; set; }

            /// <summary>
            /// ID yêu cầu tạo đề
            /// </summary>
            public Guid ExamRequestId { get; set; }

            /// <summary>
            /// Trạng thái đề thi (Draft, Published, Archived)
            /// </summary>
            public string Status { get; set; } = string.Empty;

            /// <summary>
            /// Ngày tạo đề
            /// </summary>
            public DateTime? CreatedAt { get; set; }

            /// <summary>
            /// Danh sách câu hỏi trong đề thi
            /// </summary>
            public List<ExamQuestionResponseDto> ExamQuestions { get; set; } = new();
        }

        /// <summary>
        /// DTO trả về thông tin câu hỏi trong đề thi
        /// </summary>
        public class ExamQuestionResponseDto
        {
            /// <summary>
            /// ID câu hỏi trong đề thi
            /// </summary>
            public Guid ExamQuestionId { get; set; }

            /// <summary>
            /// ID đề thi
            /// </summary>
            public Guid GeneratedExamId { get; set; }

            /// <summary>
            /// ID câu hỏi gốc (từ bảng Question)
            /// </summary>
            public Guid QuestionId { get; set; }

            /// <summary>
            /// Thứ tự câu hỏi trong đề
            /// </summary>
            public int QuestionOrder { get; set; }

            /// <summary>
            /// Điểm số cho câu hỏi này
            /// </summary>
            public decimal Points { get; set; }

            /// <summary>
            /// Thông tin chi tiết câu hỏi
            /// </summary>
            public QuestionDetailDto? QuestionDetail { get; set; }
        }

        /// <summary>
        /// DTO chi tiết câu hỏi (lồng trong ExamQuestion)
        /// </summary>
        public class QuestionDetailDto
        {
            /// <summary>
            /// ID câu hỏi
            /// </summary>
            public Guid QuestionId { get; set; }

            /// <summary>
            /// Loại câu hỏi
            /// </summary>
            public string QuestionType { get; set; } = string.Empty;

            /// <summary>
            /// Nội dung câu hỏi
            /// </summary>
            public string QuestionText { get; set; } = string.Empty;

            /// <summary>
            /// Giải thích đáp án
            /// </summary>
            public string? Explanation { get; set; }

            /// <summary>
            /// Danh sách đáp án
            /// </summary>
            public List<OptionDetailDto> Options { get; set; } = new();
        }

        /// <summary>
        /// DTO chi tiết đáp án
        /// </summary>
        public class OptionDetailDto
        {
            /// <summary>
            /// ID đáp án
            /// </summary>
            public Guid OptionId { get; set; }

            /// <summary>
            /// Nội dung đáp án
            /// </summary>
            public string Answer { get; set; } = string.Empty;

            /// <summary>
            /// Đáp án đúng hay sai
            /// </summary>
            public bool IsCorrect { get; set; }
        }
    }
}
