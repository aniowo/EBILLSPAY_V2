using System.ComponentModel.DataAnnotations;

namespace EBILLSPAY_V2.Models
{
    public class TransactionStatus
    {
        public string Nuban { get; set; }
        public string AccountName { get; set; }
        public string BillerName { get; set; }
        public string ProductName { get; set; }
        public string Amount { get; set; }
    }
}
