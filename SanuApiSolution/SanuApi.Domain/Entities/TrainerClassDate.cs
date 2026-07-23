using Dapper.Contrib.Extensions;

namespace SanuApi.Domain.Entities
{
    [Table("trainer_x_class_date")]
    public class TrainerClassDate
    {
        public int idtrainer { get; set; }

        public int idclassdate { get; set; }
    }
}
