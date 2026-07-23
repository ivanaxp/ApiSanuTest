using Dapper.Contrib.Extensions;

namespace SanuApi.Domain.Entities
{
    [Table("payment_detail")]
    public class PaymentDetail
    {
        public int id { get; set; }

        public int paymentid { get; set; }

        public int membershipid { get; set; }

        public string membershipname { get; set; }

        public decimal membershipprice { get; set; }
    }
}
