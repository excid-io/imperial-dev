using System.ComponentModel.DataAnnotations;

namespace iam.Models.Oidc4vci
{
    public class Token
    {
        [Key]
        public string Id { get; set; } = string.Empty;

        public int AuthorizationId { get; set; }
        public string clientId = string.Empty;
    }
}
