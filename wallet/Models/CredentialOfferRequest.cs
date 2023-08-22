using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Wallet.Models
{
	public class CredentialOfferRequest
	{
        public string PreAuthorizedCode { get; set; } = string.Empty;
        public string IssuerURL { get; set; } = string.Empty;
        public string CredentialType { get; set; } = string.Empty;

        [Display(Name = "Subject Id")]
        public int AuthType { get; set; }
        public int AuthClaimId { get; set; }

        public static readonly List<SelectListItem> Types = new List<SelectListItem>()
        {
            new SelectListItem {Text = "None", Value = "0"},
            new SelectListItem {Text = "Decentralized Identifier", Value = "1"},
            new SelectListItem {Text = "Certificate", Value = "2"},
        };
    }
}

