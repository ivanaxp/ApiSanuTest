namespace SanuApi.Application.DTOs.Trainer
{
    public class TrainerResponseDto
    {
        public int TrainerId { get; set; }
        public string TrainerName { get; set; }
        public string TrainerLastName { get; set; }
        public string? Email { get; set; }
        public string? Telephone { get; set; }
    }
}
