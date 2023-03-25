using System.ComponentModel.DataAnnotations;

namespace standcalcwaspnet.Models
{
    public class LoginModel
    {
        [Key]
        public int UserID { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public DateTime UserCreationDate { get; set; }
        [Required]
        public string AuthType { get; set; }

    }
}
