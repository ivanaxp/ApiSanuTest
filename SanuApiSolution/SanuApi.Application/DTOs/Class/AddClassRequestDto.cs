
namespace SanuApi.Application.DTOs.Class
{
    public class AddClassRequestDto
    {
        public string Name { get; set; }
        public List<int> MembershipIds { get; set; } = new();
        public List<ClassDateRequestDto> Dates { get; set; } = new();
    }
}
