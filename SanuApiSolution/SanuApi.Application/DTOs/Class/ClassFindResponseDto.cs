
namespace SanuApi.Application.DTOs.Class
{
    public class ClassFindResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<int> MembershipIds { get; set; } = new();
        public List<ClassDateResponseDto> Dates { get; set; } = new();
    }
}
