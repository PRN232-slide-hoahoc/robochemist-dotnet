namespace RoboChemist.Shared.DTOs.SyllabusDTOs
{
    public class SyllabusResponseDTOs
    {
        public class SyllabusDto
        {
            public Guid Id { get; set; }

            public Guid GradeId { get; set; }

            public string GradeName { get; set; } = null!;

            public Guid TopicId { get; set; }

            public int? TopicOrder {  get; set; }

            public string TopicName { get; set; } = null!;

            public string? LessonOrder { get; set; }

            public string Lesson { get; set; } = null!;

            public string? LearningObjectives { get; set; }

            public string? ContentOutline { get; set; }

            public string? KeyConcepts { get; set; }

            public bool? IsActive { get; set; }

            public DateTime? CreatedAt { get; set; }

            public DateTime? UpdatedAt { get; set; }

        }
    }
}
