
using Dapper.Contrib.Extensions;

namespace SanuApi.Domain.Entities
{
    [Table("classes")]
    public class Classes
    {
        [Key]
        public int id { get; set; }
        public string name { get; set; }

        [Write(false)]
        public List<ClassDate> Dates { get; set; } = new();

        [Write(false)]
        public List<Membership> Memberships { get; set; } = new();
    }
}
