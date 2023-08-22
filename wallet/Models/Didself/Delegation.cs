using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Wallet.Models.Didself
{
    public class Delegation
    {

        [Key]
        public int Id { get; set; }
        public int DidSelfId { get; set; }
        [DisplayName("Delegate to")]
        public int AuthType { get; set; }
        public string AuthClaim { get; set; } = string.Empty;
        public bool isEnabled { get; set; } = false;
		public string Owner { get; set; } = string.Empty;

		[NotMapped]
        public static readonly List<SelectListItem> AuthTypes = new List<SelectListItem>()
        {
            new SelectListItem {Text = "Group", Value = "0"},
            new SelectListItem {Text = "User", Value = "1"},
        };
    }
}
