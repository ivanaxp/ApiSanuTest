namespace SanuApi.Application.DTOs.Trainer
{
    public class TrainerClassWithStudentsResponseDto
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public string Day { get; set; }
        public string Hour { get; set; }
        public int Capacity { get; set; }
        public List<StudentInClassDto> Students { get; set; }
    }

    public class StudentInClassDto
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerLastName { get; set; }
    }
}
