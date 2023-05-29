using iam.Data;
using iam.Models.Oidc4vci;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;

namespace iam.Controllers
{
    [AllowAnonymous]
    public class TokenController : Controller
    {
        private readonly IamDbContext _context;

        public TokenController(IConfiguration configuration, ILogger<HomeController> logger, IamDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult Index(TokenRequest request)
        {
            if (request.grant_type == null)
            {
                return Unauthorized();
            }

            if (request.grant_type == "urn:ietf:params:oauth:grant-type:pre-authorized_code" && request!.pre_authorized_code!=null)
            {
                var authorization = _context.Authorizations.Where(q=>q.Code== request.pre_authorized_code).FirstOrDefault();
                if (authorization == null)
                {
                    return NotFound();
                }
                var token = new Token();
                token.Id = RandomString();
                token.AuthorizationId = authorization.Id;
                TempContext.Tokens.Add(token);
                return Ok(token.Id);

            }

            return Unauthorized();
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
