

namespace SanuApi.Application.DTOs.Class
{
    public class ClassFindByIdResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Day { get; set; }
        public TimeOnly Hour { get; set; }
        public int Capacity { get; set; }
    }
}
