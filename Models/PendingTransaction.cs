using System.ComponentModel.DataAnnotations;
using static EBILLSPAY_V2.Models.ApiModels;

namespace EBILLSPAY_V2.Models
{
    public class PendingTransaction
    {
        public long TransId { get; set; }
        public string billerProductId { get; set; }
        public string billerId { get; set; }
        public string billerName { get; set; }
        public decimal amount { get; set; }
        public string debitAccountNumber { get; set; }
        public string productName { get; set; }
        
    }
}
