
using Dapper.Contrib.Extensions;

namespace SanuApi.Domain.Entities
{
    [Table("class_x_customer")]
    public class ClassCustomer
    {
        public int classid { get; set; }
        public int customerid { get; set; }
        public DateTime enddate { get; set; }

        [Write(false)]
        public Classes classesName { get; set; }
    }
}
