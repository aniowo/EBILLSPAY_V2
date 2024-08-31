using System.ComponentModel.DataAnnotations;


namespace EBILLSPAY_V2.Models
{
    public class ValidateAccount
    {
        [Required(ErrorMessage = "Kindly input Account Number(Nuban)")]
        [MaxLength(10, ErrorMessage = "Account number must not exceed 10 characters.")]
        public string Nuban { get; set; }

    }
}
