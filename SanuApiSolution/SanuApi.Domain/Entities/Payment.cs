using Dapper.Contrib.Extensions;

namespace SanuApi.Domain.Entities
{
    [Table("payment")]
    public class Payment
    {
        public int id { get; set; }

        public int customerid { get; set; }

        public int periodmonth { get; set; }

        public int periodyear { get; set; }

        public decimal expectedamount { get; set; }

        public decimal paidamount { get; set; }

        public DateTime paymentdate { get; set; }

        public string? note { get; set; }

        [Write(false)]
        public List<PaymentDetail> Details { get; set; } = new();
    }
}
