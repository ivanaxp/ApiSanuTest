
using Dapper.Contrib.Extensions;

namespace SanuApi.Domain.Entities
{
    [Table("customer_x_goal")]
    public class CustomerGoal
    {
        [Key]
        public int id { get; set; }
        public int customerid { get; set; }
        public int goalid { get; set; }

        [Write(false)]
        public Goal Goal { get; set; }
    }
}
