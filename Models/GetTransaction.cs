using System.ComponentModel.DataAnnotations;

namespace EBILLSPAY_V2.Models
{
    public class GetTransaction
    {
        [Required (ErrorMessage = "Kindly input Account Number(Nuban)")]
        [MaxLength(10, ErrorMessage = "Account number must not exceed 10 characters.")]
        public string Nuban { get; set; }

        [Required(ErrorMessage = "Kindly input the Start Date")]
        public string StartDate { get; set; }

        [Required(ErrorMessage = "Kindly input the End Date")]
        public string EndDate { get; set; }
    }
}
