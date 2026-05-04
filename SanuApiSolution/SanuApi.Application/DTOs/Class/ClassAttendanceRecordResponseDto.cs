namespace SanuApi.Application.DTOs.Class
{
    public class ClassAttendanceRecordResponseDto
    {
        public int AttendanceId { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public AttendanceCustomerDto Customer { get; set; }
    }

    public class AttendanceCustomerDto
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerLastName { get; set; }
        public string Celphone { get; set; }
    }
}
