using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CloudWallet.Models
{
    public class Publickey
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "A name is required")]
        public string Name { get; set; } = string.Empty;
        public string PublicKeyJWK { get; set; } = string.Empty;

        [NotMapped]
        [Required(ErrorMessage = "Generate key before submitting")]
        public string PrivateKey { get; set; } = string.Empty;
    }

    public class PublicKeyJWKRequest
    {
        public string Name { get; set; } = string.Empty;
    }
}
