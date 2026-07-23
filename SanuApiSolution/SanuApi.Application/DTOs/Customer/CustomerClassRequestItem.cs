namespace SanuApi.Application.DTOs.Customer
{
    public class CustomerClassRequestItem
    {
        public int ClassId { get; set; }
        public List<int> ClassDateIds { get; set; } = new();
    }
}
