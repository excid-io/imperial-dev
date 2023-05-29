using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iam.Models.RelBAC
{
    public class Authorization
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "A name is required")]
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;

        [Display(Name = "Client authentication type")]
        public int AuthType { get; set; }
        public string AuthClaim { get; set; } = string.Empty;
        public string Relationships { get; set; } = string.Empty;


        [NotMapped]
        public Dictionary<string, List<string>> Relations { get; set; } = new Dictionary<string, List<string>>();

        [NotMapped]
        public static readonly List<SelectListItem> AuthTypes = new List<SelectListItem>()
        {
            new SelectListItem {Text = "None", Value = "0"},
            new SelectListItem {Text = "Client DID", Value = "1", Selected=true},
            new SelectListItem {Text = "Client public key", Value = "2"},
            new SelectListItem {Text = "Client EORI", Value = "3"},
        };

    }
}
