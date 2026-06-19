using SanuApi.Application.DTOs.Class;

namespace SanuApi.Application.DTOs.Membership
{
    public class ClassInMembershipDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<ClassDateResponseDto> Dates { get; set; } = new();
    }
}
