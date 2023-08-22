using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Wallet.Models
{
    public class Certificate
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "A Common Name is required")]
        [DisplayName("Subject CN")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "A certificate is required")]
        [DisplayName("PEM Certificate without --BEGIN and --END lines")]
        public string X509 { get; set; } = string.Empty;

        [DisplayName("PEM private key without --BEGIN and --END lines")]
        [Required(ErrorMessage = "A private key is required")]
        public string PrivateKey { get; set; } = string.Empty;

        public string Owner { get; set; } = string.Empty;
    }
}
