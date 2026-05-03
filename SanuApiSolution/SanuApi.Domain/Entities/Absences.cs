using Dapper.Contrib.Extensions;

namespace SanuApi.Domain.Entities
{
    [Table("absences")]
    public class Absences
    {
        [Key]
        public int id { get; set; }
        public int customerid { get; set; }
        public int classid { get; set; }
        public DateTime dateabsence { get; set; }
        public string? status { get; set; }
    }
}
