namespace RoboChemist.Shared.DTOs.UserDTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Fullname { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string? Phone { get; set; }

        public string? Role { get; set; }

        public string? Status { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? CreatedAt { get; set; }
    }

}
