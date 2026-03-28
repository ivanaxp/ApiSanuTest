namespace SanuApi.Application.DTOs.Class
{
    public class ClassWithCustomersResponseDto
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public string Day { get; set; }
        public string Hour { get; set; }
        public int Capacity { get; set; }
        public List<CustomerInClassDto> Customers { get; set; }
    }

    public class CustomerInClassDto
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerLastName { get; set; }
        public string Celphone { get; set; }
    }
}
