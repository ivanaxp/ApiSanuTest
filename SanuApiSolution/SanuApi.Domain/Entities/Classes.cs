
using Dapper.Contrib.Extensions;

namespace SanuApi.Domain.Entities
{
    [Table("classes")]
    public class Classes
    {
        [Key]
        public int id { get; set; }
        public int capacity { get; set; }
        public string name { get; set; }
        public string day { get; set; }
        public string hour { get; set; }
    }
}
