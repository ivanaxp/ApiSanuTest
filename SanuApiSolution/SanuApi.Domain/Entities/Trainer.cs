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

        public string? email { get; set; }

        public string? telephone { get; set; }

        public DateTime? endDate { get; set; }
    }
}
