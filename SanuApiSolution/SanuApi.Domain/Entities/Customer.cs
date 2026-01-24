using Dapper.Contrib.Extensions;
namespace SanuApi.Domain.Entities {
    [Table("customer")]
    public class Customer
    {
        public int id { get; set; }
        public string customername { get; set; }
        public string customerlastname { get; set; }
        public DateTime datebirth { get; set; }
        public int dni { get; set; }
        public string celphone { get; set; }
        public string address { get; set; }
        public string comentaries { get; set; }
        public bool? ismale { get; set; }
        public DateTime? fechabaja { get; set; }
        public DateTime? fechaalta { get; set; }

        [Write(false)]
        public List<CustomerGoal> customerGoals { get; set; }

        [Write(false)]
        public List<CustomerMembership> customerMembership { get; set; }

        [Write(false)]
        public HealthCustomer healthCustomer { get; set; }

        [Write(false)]
        public ClassCustomer classesCustomer { get; set; }

    }
}

