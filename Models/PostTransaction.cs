using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EBILLSPAY_V2.Models
{
    public class PostTransaction
    {

        public string Nuban { get; set; }

        public string AccountName { get; set; }

        public string BillerId { get; set; }
        public string SelectedBiller { get; set; }
        public string ProductId { get; set; }
        public string SelectedProduct { get; set; }


        [Required(ErrorMessage = "Kindly Select a Biller")]
        public List<GetBiller> Biller { get; set; } = new List<GetBiller>();


        [Required(ErrorMessage = "Kindly Select a Product")]
        public List<GetBillerProducts> Products { get; set; } = new List<GetBillerProducts>();


        public List<GetFieldName> FieldNames { get; set; } = new List<GetFieldName>();

        public Dictionary<string, string> DynamicFields { get; set; } = new Dictionary<string, string>();


        [Required(ErrorMessage = "Kindly Input Transaction Amount")]
        public string Amount { get; set; }

        public string Fee { get; set; }
    }
}
