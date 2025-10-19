namespace RoboChemist.Shared.DTOs.TopicDTOs
{
    public class TopicDTOs
    {
        public class GetTopicDto
        {
            public Guid Id { get; set; }
            public Guid GradeId { get; set; }
            public string GradeName { get; set; } = string.Empty;
            public int SortOrder { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
        }

        public class CreateTopicDto
        {
            public Guid GradeId { get; set; }
            public int SortOrder { get; set; }
            public string Name { get; set; } = string.Empty;
            public string? Description { get; set; } = string.Empty;
        }
    }
}
