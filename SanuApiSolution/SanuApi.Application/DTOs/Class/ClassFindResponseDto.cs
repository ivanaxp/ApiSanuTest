
namespace SanuApi.Application.DTOs.Class
{
    public class ClassFindResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? IdMembership { get; set; }
        public List<ClassDateResponseDto> Dates { get; set; } = new();
    }
}
