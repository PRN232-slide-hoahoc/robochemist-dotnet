using System.ComponentModel.DataAnnotations;
using RoboChemist.Shared.Common.Constants;

namespace RoboChemist.Shared.DTOs.ExamServiceDTOs
{
    /// <summary>
    /// DTOs cho Matrix (Ma trận đề thi)
    /// </summary>
    public class MatrixDTOs
    {
        /// <summary>
        /// DTO để tạo mới một ma trận đề thi
        /// </summary>
        public class CreateMatrixDto
        {
            /// <summary>
            /// Tên ma trận đề (VD: "Ma trận đề kiểm tra giữa kỳ Hóa 10")
            /// </summary>
            [Required(ErrorMessage = "Tên ma trận là bắt buộc")]
            [StringLength(200, MinimumLength = 5, ErrorMessage = "Tên ma trận phải từ 5-200 ký tự")]
            public string Name { get; set; } = string.Empty;

            /// <summary>
            /// Tổng số câu hỏi trong ma trận
            /// </summary>
            [Range(1, 200, ErrorMessage = "Tổng số câu hỏi phải từ 1-200")]
            public int TotalQuestion { get; set; }

            /// <summary>
            /// Danh sách chi tiết phân bổ câu hỏi theo chủ đề
            /// </summary>
            [Required(ErrorMessage = "Phải có ít nhất 1 chi tiết ma trận")]
            [MinLength(1, ErrorMessage = "Phải có ít nhất 1 chi tiết ma trận")]
            public List<CreateMatrixDetailDto> MatrixDetails { get; set; } = new();
        }

        /// <summary>
        /// DTO để tạo chi tiết ma trận (phân bổ câu hỏi theo chủ đề)
        /// </summary>
        public class CreateMatrixDetailDto
        {
            /// <summary>
            /// ID của chủ đề (Guid - từ SlidesService)
            /// </summary>
            [Required(ErrorMessage = "TopicId là bắt buộc")]
            public Guid TopicId { get; set; }

            /// <summary>
            /// Loại câu hỏi (MultipleChoice, TrueFalse, FillBlank, Essay)
            /// </summary>
            [Required(ErrorMessage = "Loại câu hỏi là bắt buộc")]
            [RegularExpression("^(MultipleChoice|TrueFalse|FillBlank|Essay)$", 
                ErrorMessage = "Loại câu hỏi phải là: MultipleChoice, TrueFalse, FillBlank, hoặc Essay")]
            public string QuestionType { get; set; } = string.Empty;

            /// <summary>
            /// Mức độ câu hỏi (NhanBiet, ThongHieu, VanDung, VanDungCao)
            /// </summary>
            [StringLength(50, ErrorMessage = "Mức độ câu hỏi không được vượt quá 50 ký tự")]
            [RegularExpression("^(NhanBiet|ThongHieu|VanDung|VanDungCao)$", 
                ErrorMessage = "Mức độ câu hỏi phải là: NhanBiet, ThongHieu, VanDung, hoặc VanDungCao")]
            public string? Level { get; set; }

            /// <summary>
            /// Số lượng câu hỏi cho chủ đề này
            /// </summary>
            [Range(1, 50, ErrorMessage = "Số lượng câu hỏi phải từ 1-50")]
            public int QuestionCount { get; set; }
        }

        /// <summary>
        /// DTO để cập nhật ma trận đề thi
        /// </summary>
        public class UpdateMatrixDto
        {
            /// <summary>
            /// Tên ma trận đề
            /// </summary>
            [Required(ErrorMessage = "Tên ma trận là bắt buộc")]
            [StringLength(200, MinimumLength = 5, ErrorMessage = "Tên ma trận phải từ 5-200 ký tự")]
            public string Name { get; set; } = string.Empty;

            /// <summary>
            /// Tổng số câu hỏi
            /// </summary>
            [Range(1, 200, ErrorMessage = "Tổng số câu hỏi phải từ 1-200")]
            public int TotalQuestion { get; set; }

            /// <summary>
            /// Trạng thái hoạt động
            /// </summary>
            public bool IsActive { get; set; } = true;

            /// <summary>
            /// Danh sách chi tiết ma trận
            /// </summary>
            [Required(ErrorMessage = "Phải có ít nhất 1 chi tiết ma trận")]
            [MinLength(1, ErrorMessage = "Phải có ít nhất 1 chi tiết ma trận")]
            public List<CreateMatrixDetailDto> MatrixDetails { get; set; } = new();
        }

        /// <summary>
        /// DTO trả về thông tin ma trận đề thi
        /// </summary>
        public class MatrixResponseDto
        {
            /// <summary>
            /// ID của ma trận
            /// </summary>
            public Guid MatrixId { get; set; }

            /// <summary>
            /// Tên ma trận
            /// </summary>
            public string Name { get; set; } = string.Empty;

            /// <summary>
            /// Tổng số câu hỏi
            /// </summary>
            public int TotalQuestion { get; set; }

            /// <summary>
            /// Trạng thái hoạt động
            /// </summary>
            public bool IsActive { get; set; }

            /// <summary>
            /// Người tạo
            /// </summary>
            public Guid? CreatedBy { get; set; }

            /// <summary>
            /// Ngày tạo
            /// </summary>
            public DateTime? CreatedAt { get; set; }

            /// <summary>
            /// Người cập nhật
            /// </summary>
            public Guid? UpdatedBy { get; set; }

            /// <summary>
            /// Ngày cập nhật
            /// </summary>
            public DateTime? UpdatedAt { get; set; }

            /// <summary>
            /// Danh sách chi tiết phân bổ câu hỏi
            /// </summary>
            public List<MatrixDetailResponseDto> MatrixDetails { get; set; } = new();
        }

        /// <summary>
        /// DTO trả về chi tiết ma trận
        /// </summary>
        public class MatrixDetailResponseDto
        {
            /// <summary>
            /// ID chi tiết ma trận
            /// </summary>
            public Guid MatrixDetailsId { get; set; }

            /// <summary>
            /// ID chủ đề (Guid - tham chiếu đến SlidesService)
            /// </summary>
            public Guid TopicId { get; set; }

            /// <summary>
            /// Tên chủ đề (lấy từ SlidesService)
            /// </summary>
            public string TopicName { get; set; } = string.Empty;

            /// <summary>
            /// Loại câu hỏi
            /// </summary>
            public string QuestionType { get; set; } = string.Empty;

            /// <summary>
            /// Mức độ câu hỏi (NhanBiet, ThongHieu, VanDung, VanDungCao)
            /// </summary>
            public string? Level { get; set; }

            /// <summary>
            /// Số lượng câu hỏi
            /// </summary>
            public int QuestionCount { get; set; }

            /// <summary>
            /// Trạng thái hoạt động
            /// </summary>
            public bool IsActive { get; set; }
        }

        /// <summary>
        /// DTO trả về thông tin cơ bản của ma trận (không bao gồm chi tiết)
        /// </summary>
        public class MatrixBasicDto
        {
            /// <summary>
            /// ID của ma trận
            /// </summary>
            public Guid MatrixId { get; set; }

            /// <summary>
            /// Tên ma trận
            /// </summary>
            public string Name { get; set; } = string.Empty;

            /// <summary>
            /// Tổng số câu hỏi
            /// </summary>
            public int TotalQuestion { get; set; }

            /// <summary>
            /// Trạng thái hoạt động
            /// </summary>
            public bool IsActive { get; set; }

            /// <summary>
            /// Ngày tạo
            /// </summary>
            public DateTime? CreatedAt { get; set; }

            /// <summary>
            /// Người tạo
            /// </summary>
            public Guid? CreatedBy { get; set; }
        }
    }
}
