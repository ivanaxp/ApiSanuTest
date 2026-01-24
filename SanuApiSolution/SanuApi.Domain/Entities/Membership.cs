using Dapper.Contrib.Extensions;
namespace SanuApi.Domain.Entities
{
    [Table("membership")]
    public class Membership
    {
        public int id { get; set; }
        public string name { get; set; }
        public decimal price { get; set; }
        public int frecuency { get; set; }
    }
}
