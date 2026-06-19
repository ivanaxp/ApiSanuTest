
using Dapper.Contrib.Extensions;

namespace SanuApi.Domain.Entities
{
    [Table("classes")]
    public class Classes
    {
        [Key]
        public int id { get; set; }
        public string name { get; set; }
        public int? idmembership { get; set; }

        [Write(false)]
        public List<ClassDate> Dates { get; set; } = new();
    }
}
