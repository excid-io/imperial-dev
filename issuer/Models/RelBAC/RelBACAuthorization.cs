 using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;

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

        public bool Revoked { get; set; } = false;


        [NotMapped]
        public Dictionary<string, List<string>> Relations { get; set; } = new Dictionary<string, List<string>>();

        [NotMapped]
        public static readonly List<SelectListItem> AuthTypes = new List<SelectListItem>()
        {
            new SelectListItem {Text = "None", Value = "0"},
            new SelectListItem {Text = "Decentralized Identifier", Value = "1", Selected=true},
            new SelectListItem {Text = "X509 Certificate", Value = "2"},
        };

        [NotMapped]
        public string Issuer { get; set; } = string.Empty;

        [NotMapped]
        public string Host { get; set; } = string.Empty;
        [NotMapped]
        public string SubjectId { get; set; } = string.Empty;

        [NotMapped]
        public JwtPayload Credential
        {
            get
            {
                var iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var exp = DateTimeOffset.UtcNow.AddDays(15).ToUnixTimeSeconds();
                var iss = Issuer;
                string jti = RandomString;


                var vc = new Dictionary<String, Object>()
                {
                    {"@context", new String[]{ "https://www.w3.org/2018/credentials/v1", "https://excid-io.github.io/imperial/contexts/v1"}},
                    {"type", new String[]{ "VerifiableCredential", "RelationsCredential" } },
                    {"credentialSubject", new Dictionary<String, Object>()
                        {
                        {"id", SubjectId},
                        {"relationships",Relationships!=string.Empty?JsonSerializer.Deserialize<List<Dictionary<string, string>>>(Relationships)!:""}
                        }
                    },
                    {"credentialStatus", new Dictionary<String, Object>()
                        {
                            {"type", "RevocationList2021Status" },
                            { "statusListIndex", Id},
                            {"statusListCredential", Host+"/credential/status" }
                        }
                    }

                };

                var payload = new JwtPayload(iss, null, new List<Claim>(), null, null)
                {
                    { "jti", jti },
                    { "sub", SubjectId },
                    { "iat", iat },
                    { "exp", exp },
                    { "vc", vc }
                };

                return payload;

            }
        }

        [NotMapped]
        private string RandomString
        {
            get
            {
                // Create a new instance of the RNGCryptoServiceProvider class
                var rng = RandomNumberGenerator.Create();

                // Create a byte array to store the random bytes
                byte[] bytes = new byte[16];

                // Fill the array with random bytes
                rng.GetBytes(bytes);

                // Convert the byte array to a hexadecimal string
                string hex = Convert.ToHexString(bytes);
                return hex;
            }
        }
    }
}
