using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace Wallet.Models.Oidc4vci
{
    public class CredentialOffer
    {
        [JsonPropertyName("credential_issuer")]
        public string CredentialIssuer { get; set; } = string.Empty;

        [JsonPropertyName("credentials")]
        public List<OfferedCredential> Credentials { get; set; } = new List<OfferedCredential> { new OfferedCredential() };

        [JsonPropertyName("grants"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Grant? Grants { get; set; }
    }

    public class OfferedCredential
    {
        [JsonPropertyName("format")]
        public string Format { get; set; } = "jwt_vc";

        [JsonPropertyName("types")]
        public List<string> Types { get; set; } = new List<string> { "VerifiableCredential" };
    }

    public class Grant
    {
        [JsonPropertyName("authorization_code"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public AuthorizationCode? AuthorizationCode { get; set; }

        [JsonPropertyName("urn:ietf:params:oauth:grant-type:pre-authorized_code"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public PreAuthorizedCode? PreAuthorizedCode { get; set; }

        [JsonPropertyName("urn:ietf:params:oauth:client-assertion-type:jwt-bearer"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? JWTBarer { get; set; }
    }

    public class PreAuthorizedCode
    {
        [JsonPropertyName("pre-authorized_code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("user_pin_required")]
        public bool UserPinRequired { get; set; } = false;
    }

    public class AuthorizationCode
    {
        [JsonPropertyName("issuer_state")]
        public string IssuerState { get; set; } = string.Empty;
    }

}
