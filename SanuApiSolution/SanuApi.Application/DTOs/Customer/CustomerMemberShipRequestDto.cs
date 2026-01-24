
namespace SanuApi.Application.DTOs.Customer
{
    public class CustomerMemberShipRequestDto
    {
        public int IdMembership { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

    }
}
