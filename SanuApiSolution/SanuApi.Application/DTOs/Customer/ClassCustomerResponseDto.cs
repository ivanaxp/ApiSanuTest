namespace SanuApi.Application.DTOs.Customer
{
    public class ClassCustomerResponseDto
    {
        public int ClassId { get; set; }
        public int CustomerId { get; set; }
        public string Name { get; set; }
        public int? IdMembership { get; set; }
        public int ClassDateId { get; set; }
        public string Day { get; set; }
        public string Hour { get; set; }
    }
}
