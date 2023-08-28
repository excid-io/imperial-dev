
using iam.Data;
using iam.Models.Oidc4vci;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


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
            //System.Diagnostics.Debug.WriteLine("Received:" + request.proof.jwt);
            //var tokenHandler = new JwtSecurityTokenHandler();
            //var proof = tokenHandler.ReadJwtToken(request.proof.jwt);
            StringValues code;
            Request.Headers.TryGetValue("Authorization", out code);
            if (code.Count > 0) {
                string tokenId = code!.First()!.Split(" ")[1];
                System.Diagnostics.Debug.WriteLine("Code:" + tokenId);
                string? did = TempContext.Tokens.FirstOrDefault(t => t.Id == tokenId)!.clientId;
                int? auth_id = TempContext.Tokens.FirstOrDefault(t => t.Id == tokenId)!.AuthorizationId;
                if (did != null && auth_id!=null)
                {
                    var authorization = _context.Authorizations.FirstOrDefault(q=>q.Id== auth_id);
                    if (authorization != null)
                    {
                        authorization.IssuerURL = _configuration["jwt_vc_iss"] ?? "";
                        authorization.SubjectId = did;
                        var credential = authorization.Credential;
                        string vc = JWTSignedCredential(credential);
                        return Ok(vc);
                    }
                }
            }
            return NotFound();
        }

        public IActionResult Status()
        {
            byte[] bytes = new byte[2000]; //It holds 16K VCs. This the the mimimum size according to https://w3c-ccg.github.io/vc-status-list-2021/#revocationlist2021status
            var exp = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds();
            List<int> indexes = _context.Authorizations.Where(q => q.Revoked == true).Select(q => q.Id).ToList();
            foreach (int bitindex in indexes)
            {
                int _bitindex = bitindex % 16000;
                int index = _bitindex / 8;
                int bit = _bitindex % 8;
                byte mask = (byte)(1 << bit);
                bytes[index] |= mask;
            }
            var s = Convert.ToBase64String(bytes);
            /*
             * Created and sign the revocation list
             */
            var response = createRevocationList(s);
            return Ok(response);
        }

        public IActionResult TestStatus()
        {
            /*
             * Still in progress
             * It generates a revocation list with the
             * 9th VC revoked
             */

            byte[] bytes = new byte[2000]; //It holds 16K VCs. This the the mimimum size according to https://w3c-ccg.github.io/vc-status-list-2021/#revocationlist2021status
            int bitindex = 9; //VC with index 9 is revoked. Note the first index is 0
            int index = bitindex / 8;
            int bit = bitindex % 8;
            byte mask = (byte)(1 << bit);
            bytes[1] |= mask;
            var s = Convert.ToBase64String(bytes);
            /*
             * Created and sign the revocation list
             */
            var response = createRevocationList(s);
            return Ok(response);
        }

        private string createRevocationList(String bitstring64)
        {

            var iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var exp = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds();
            var iss = _configuration["jwt_vc_iss"];
            var vc = new Dictionary<String, Object>()
            {
                {"@context", new String[]{ "https://www.w3.org/2018/credentials/v1", "https://w3id.org/vc/status-list/v1"}},
                {"type", new String[]{ "VerifiableCredential", "StatusList2021Credential" } },
                {
                    "credentialSubject", new Dictionary<String, Object>()
                    {
                        {"type","RevocationList2021" },
                        {"encodedList",bitstring64} //FIX that, it must be GZIPed
                    }
                }

            };
            var payload = new JwtPayload(iss, null, new List<Claim>(), null, null)
                {
                    { "iat", iat },
                    { "exp", exp },
                    { "vc", vc }
            };

            return JWTSignedCredential(payload);

        }

        private string JWTSignedCredential(JwtPayload credential)
        {
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
            var jwtToken = new JwtSecurityToken(jwtHeader, credential);
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            return jwtTokenHandler.WriteToken(jwtToken);
        }

    }

}

   

