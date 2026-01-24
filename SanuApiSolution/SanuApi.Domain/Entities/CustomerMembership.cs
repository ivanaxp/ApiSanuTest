
using Dapper.Contrib.Extensions;

namespace SanuApi.Domain.Entities
{
    [Table("customer_x_membership")]
    public class CustomerMembership
    {
        [Key]
        public int id { get; set; }
        public int customerid { get; set; }
        public int membershipid { get; set; }
        public DateTime startdate { get; set; }
        public DateTime? enddate { get; set; }

        [Write(false)]
        public Membership Membership { get; set; }
    }
}
