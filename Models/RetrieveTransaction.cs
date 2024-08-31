using System.ComponentModel.DataAnnotations;

namespace EBILLSPAY_V2.Models
{
    public class RetrieveTransaction
    {
        [Required(ErrorMessage = "Kindly input Branch Code")]
        [MaxLength(3, ErrorMessage = "Branch Code must not exceed 3 characters.")]
        public string BranchCode { get; set; }
    }
}
