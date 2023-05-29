
using iam.Data;
using iam.Models.Oidc4vci;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Specialized;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Text;
using iam.Models.RelBAC;
using System.Security.Cryptography;

namespace iam.Controllers
{
    [AllowAnonymous]
    public class CredentialController : Controller
    {
        private readonly IamDbContext _context;
        private readonly IConfiguration _configuration;

        public CredentialController(IConfiguration configuration, ILogger<HomeController> logger, IamDbContext context)
        {
            _context = context;
            _configuration = configuration;
        }
        [HttpPost]
        public IActionResult Index([FromBody] CredentialRequest request)
        {
            System.Diagnostics.Debug.WriteLine("Received:" + request.proof.jwt);
            var tokenHandler = new JwtSecurityTokenHandler();
            var proof = tokenHandler.ReadJwtToken(request.proof.jwt);
            StringValues code;
            Request.Headers.TryGetValue("Authorization", out code);
            if (proof != null && code.Count > 0) {
                string tokenId = code!.First()!.Split(" ")[1];
                System.Diagnostics.Debug.WriteLine("Code:" + tokenId);
                string? did = proof.Header["kid"].ToString();
                int? auth_id = TempContext.Tokens.FirstOrDefault(t => t.Id == tokenId)!.AuthorizationId;
                if (did != null && auth_id!=null)
                {
                    var authorization = _context.Authorizations.FirstOrDefault(q=>q.Id== auth_id);
                    if (authorization != null)
                    {
                        var vc = Issue(authorization, did);
                        System.Diagnostics.Debug.WriteLine("Issuing:" + vc);
                        return Ok(vc);
                    }
                }
            }
            return NotFound();
        }

        private string Issue(Authorization authorization, string did)
        {
            //TODO chech if null
            var iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var exp = DateTimeOffset.UtcNow.AddDays(15).ToUnixTimeSeconds();
            var iss = _configuration["iss_url"];
            string jti = RandomString();
            System.Diagnostics.Debug.WriteLine("Relationships:" + authorization.Relationships);
            var relationships = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(authorization.Relationships);

            var vc = new Dictionary<String, Object>()
            {
                {"@context", new String[]{ "https://www.w3.org/2018/credentials/v1", "https://excid-io.github.io/imperial/contexts/v1"}},
                {"type", new String[]{ "VerifiableCredential", "RelationsCredential" } },
                {
                    "credentialSubject", new Dictionary<String, Object>()
                    {
                        {"relationships",relationships!}
                    }
            }

            };

            var payload = new JwtPayload(iss, null, new List<Claim>(), null, null)
            {
                { "jti", jti },
                { "sub", did },
                { "iat", iat },
                { "exp", exp },
                { "vc", vc }
            };
            var signingJWK = new JsonWebKey(_configuration["jwk"]);
            var publicJWK = new JsonWebKey(_configuration["jwk"]);
            publicJWK.D = null;
            var jwtHeader = new JwtHeader(
                new SigningCredentials(
                    key: signingJWK,
                    algorithm: SecurityAlgorithms.EcdsaSha256)
                )
            {
                { "jwk", publicJWK }
            };
            var jwtToken = new JwtSecurityToken(jwtHeader, payload);
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            return jwtTokenHandler.WriteToken(jwtToken);
        }

        private string RandomString()
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

   

