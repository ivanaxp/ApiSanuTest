
using Dapper.Contrib.Extensions;

namespace SanuApi.Domain.Entities
{
    [Table("healthcustomer")]
    public class HealthCustomer
    {
        public int id { get; set; }

        public int customerid { get; set; }

        public decimal? heigth { get; set; }

        public decimal? weight { get; set; }

        public string alergics { get; set; }

        public string medicalCondicion { get; set; }
    }
}
