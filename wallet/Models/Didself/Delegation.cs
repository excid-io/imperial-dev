using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace CloudWallet.Models.Didself
{
    public class Delegation
    {

        [Key]
        public int Id { get; set; }
        public int DidSelfId { get; set; }
        public int AuthType { get; set; }
        public string AuthClaim { get; set; } = string.Empty;
        public bool isEnabled { get; set; }

        [NotMapped]
        public static readonly List<SelectListItem> AuthTypes = new List<SelectListItem>()
        {
            new SelectListItem {Text = "None", Value = "0"},
            new SelectListItem {Text = "Client DID", Value = "1"},
            new SelectListItem {Text = "Client public key", Value = "2", Selected=true},
            new SelectListItem {Text = "Client EORI", Value = "3"},
        };
    }
}
