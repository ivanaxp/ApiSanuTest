using SanuApi.Application.DTOs.Class;

namespace SanuApi.Application.DTOs.Trainer
{
    public class TrainerClassWithStudentsResponseDto
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public List<ClassDateResponseDto> Dates { get; set; } = new();
        public List<StudentInClassDto> Students { get; set; }
    }

    public class StudentInClassDto
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerLastName { get; set; }
    }
}
