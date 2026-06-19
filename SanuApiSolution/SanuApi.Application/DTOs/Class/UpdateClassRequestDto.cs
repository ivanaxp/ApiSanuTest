namespace SanuApi.Application.DTOs.Class
{
    public class UpdateClassRequestDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? IdMembership { get; set; }
        public List<ClassDateRequestDto> Dates { get; set; } = new();
    }
}
