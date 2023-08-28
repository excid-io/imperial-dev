using iam.Data;
using iam.Models.Oidc4vci;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text.Json;

namespace iam.Controllers
{
    [AllowAnonymous]
    public class TokenController : Controller
    {
        private readonly IamDbContext _context;
        private readonly ILogger<TokenController> _logger;

        public TokenController(IConfiguration configuration, ILogger<TokenController> logger, IamDbContext context)
        {
            _context = context;
            _logger = logger;
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
                token.clientId = request.client_id;
                _logger.LogInformation("Token:" + JsonSerializer.Serialize(token));
                TempContext.Tokens.Add(token);
                return Json(new { access_token = token.Id, token_type = "Bearer", expires_in = 3600 });

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
