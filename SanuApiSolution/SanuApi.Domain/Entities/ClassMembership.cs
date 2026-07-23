
using Dapper.Contrib.Extensions;
namespace SanuApi.Domain.Entities
{
    [Table("class_x_membership")]
    public class ClassMembership
    {
        public int classid { get; set; }
        public int membershipid { get; set; }
    }
}
