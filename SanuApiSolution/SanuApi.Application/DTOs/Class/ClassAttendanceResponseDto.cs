namespace SanuApi.Application.DTOs.Class
{
    public class ClassAttendanceResponseDto
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public DateTime Date { get; set; }
        public int Capacity { get; set; }
        public int TotalEnrolled { get; set; }
        public int FreeSpotsToday { get; set; }
        public List<StudentAttendanceDto> Students { get; set; }
    }

    public class StudentAttendanceDto
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerLastName { get; set; }
        /// <summary>null si aún no fue registrado ese día.</summary>
        public string? Status { get; set; }
    }
}
