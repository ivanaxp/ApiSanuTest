namespace SanuApi.Application.DTOs.Class
{
    public class UpdateClassRequestDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<int> MembershipIds { get; set; } = new();
        public List<ClassDateRequestDto> Dates { get; set; } = new();
    }
}
