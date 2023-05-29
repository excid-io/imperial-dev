using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;

namespace iam.Models.Oidc4vci
{
    public class TokenRequest
    {
        public string grant_type { get; set; } = string.Empty;

        [BindProperty(Name = "pre-authorized_code")]
        public string pre_authorized_code { get; set;} = string.Empty;
    }
}
