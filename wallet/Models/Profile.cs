using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CloudWallet.Models
{
    public class Profile
    {
        [Key]        
        public int ID { get; set; }

        [Required(ErrorMessage = "A profile name is required")]
        [DisplayName("Profile Name")]
        public string ProfileName { get; set; } = string.Empty;
        public string PublicKeyJWK { get; set; } = string.Empty;

        [NotMapped]
        [Required(ErrorMessage = "Generate key before submitting")]
        public string PrivateKey { get; set; } = string.Empty;
    }
}
