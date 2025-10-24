using System.ComponentModel.DataAnnotations;

namespace RoboChemist.Shared.DTOs.SyllabusDTOs
{
    public class SyllabusRequestDTOs
    {
        /// <summary>
        /// Data transfer object for creating or updating a syllabus
        /// </summary>
        public class CreateSyllabusRequestDto
        {
            /// <summary>
            /// ID of the topic this syllabus belongs to
            /// </summary>
            [Required(ErrorMessage = "TopicId là bắt buộc")]
            public Guid TopicId { get; set; }

            /// <summary>
            /// Order of the lesson in the syllabus sequence (1-50)
            /// </summary>
            [Range(1, 50, ErrorMessage = "Thứ tự bài học phải từ 1 đến 50")]
            public int? LessonOrder { get; set; }

            /// <summary>
            /// Title/name of the lesson
            /// </summary>
            [Required(ErrorMessage = "Tên bài học là bắt buộc")]
            [MaxLength(500, ErrorMessage = "Tên bài học không được vượt quá 500 ký tự")]
            public string Lesson { get; set; } = null!;

            /// <summary>
            /// Learning objectives for this lesson
            /// </summary>
            [Required(ErrorMessage = "Mục tiêu học tập là bắt buộc")]
            public string? LearningObjectives { get; set; }

            /// <summary>
            /// Detailed content outline for the lesson
            /// </summary>
            [Required(ErrorMessage = "Đề cương nội dung là bắt buộc")]
            public string? ContentOutline { get; set; }

            /// <summary>
            /// Key concepts covered in this lesson
            /// </summary>
            [Required(ErrorMessage = "Khái niệm chính là bắt buộc")]
            public string? KeyConcepts { get; set; }
        }
    }
}
