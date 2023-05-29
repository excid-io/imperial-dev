using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace CloudWallet.Models.Didself
{
    public class Didself
    {

        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "A name is required")]
        public string Name { get; set; } = string.Empty;
        public string Did { get; set; } = string.Empty;
        public string AuthentiationPublicKeyJWK { get; set; } = string.Empty;
        public string AuthentiationPrivateKeyJWK { get; set; } = string.Empty;
        public string IdentifierPublicKeyJWK { get; set; } = string.Empty;
        public string IdentifierPrivateKeyJWK { get; set; } = string.Empty;
    }
}
