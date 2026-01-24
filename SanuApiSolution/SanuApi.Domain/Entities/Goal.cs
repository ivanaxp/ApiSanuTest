
using Dapper.Contrib.Extensions;

namespace SanuApi.Domain.Entities
{
    [Table("goal")]
    public class Goal
    {
        public int id { get; set; }
        public string goalname { get; set; }
        public DateTime? fechaBaja { get; set; }
    }
}
