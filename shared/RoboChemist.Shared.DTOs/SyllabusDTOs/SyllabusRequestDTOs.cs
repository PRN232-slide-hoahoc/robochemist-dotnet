namespace RoboChemist.Shared.DTOs.SyllabusDTOs
{
    public class SyllabusRequestDTOs
    {
        public class CreateSyllabusRequestDto
        {
            public Guid TopicId { get; set; }

            public int? LessonOrder { get; set; }

            public string Lesson { get; set; } = null!;

            public string? LearningObjectives { get; set; }

            public string? ContentOutline { get; set; }

            public string? KeyConcepts { get; set; }
        }
    }
}
