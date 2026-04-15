
using Dapper.Contrib.Extensions;
namespace SanuApi.Domain.Entities
{
    [Table("trainer_x_classes")]
    public class TrainerClasses
    {
        public int idtrainer { get; set; }

        public int idclass { get; set; }
    }
}
