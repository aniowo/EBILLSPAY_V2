namespace EBILLSPAY_V2.Models
{
    public class TransactionHistory
    {

        public string SessionId { get; set; }
        public string Narration { get; set; }
        public string Originator { get; set; }
        public string Amount { get; set; }
        public string  Beneficiary { get; set; }
        public string Description { get; set; }
        public DateTime DateInitiated { get; set; }
       
    }
}
