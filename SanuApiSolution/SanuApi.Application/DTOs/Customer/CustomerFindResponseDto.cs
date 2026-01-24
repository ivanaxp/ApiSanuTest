using System.Reflection;

namespace SanuApi.Application.DTOs.Customer {
    public class CustomerFindResponseDto
    {
        public int IdCoustomer { get; set; }
        public string CoustomerName { get; set; }
        public string CoustomerLastName { get; set; }
        public DateTime DateBirth { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Dni { get; set; }
        public string Celphone { get; set; }
        public string Adress { get; set; }
        public Gender Gender { get; set; }
        public string Comentaries { get; set; }
        public string GoalName { get; set; }
    }
}
