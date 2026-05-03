namespace SanuApi.Application.DTOs.Customer
{
    public class CustomerAbsenceResponseDto
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public DateTime DateAbsence { get; set; }
        public string Status { get; set; }
    }
}
