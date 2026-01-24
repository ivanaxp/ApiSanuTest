namespace SanuApi.Application.DTOs.Customer
{
    public class CustomerMemberShipResponseDto
    {
        public int MembershipId { get; set; }
        public string MembershipName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}