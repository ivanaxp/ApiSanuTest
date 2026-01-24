using SanuApi.Application.DTOs.Goal;

namespace SanuApi.Application.DTOs.Customer
{
    public class CutsomerFindByIdResponseDto
    {
        public int IdCustomer { get; set; }
        public string CustomerName { get; set; }
        public string CustomerLastName { get; set; }
        public DateTime DateBirth { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Dni { get; set; }
        public string Celphone { get; set; }
        public string Address { get; set; }
        public Gender Gender { get; set; }        
        public string Comentaries { get; set; }
        public List<GoalFindResponseDto> Goals { get; set; }
        public List<CustomerMemberShipResponseDto> Memberships { get; set; }
        public HealthCustomerDto HealthCustomer { get; set; }
        public List<ClassCustomerResponseDto> Classes { get; set; }

    }
}
