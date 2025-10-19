namespace RoboChemist.Shared.DTOs.GradeDTOs
{
    public class GradeDTOs
    {
        public class GradeDto
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
        }   
    }
}
