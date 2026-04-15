using Dapper.Contrib.Extensions;

namespace SanuApi.Domain.Entities
{
    [Table("trainer")]
    public class Trainer
    {
        [Key]
        public int id { get; set; }

        public string name { get; set; }

        public string lastName { get; set; }
    }
}
