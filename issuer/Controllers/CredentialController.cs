
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
                        authorization.IssuerURL = _configuration["iss_url"] ?? "";
                        authorization.SubjectId = did;
                        var credential = authorization.Credential;
                        string vc = JWTSignedCredential(credential);
                        return Ok(vc);
                    }
                }
            }
            return NotFound();
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

   

