using iam.Data;
using iam.Models.Oidc4vci;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Policy;
using System.Text.Json;

namespace iam.Controllers
{
    [AllowAnonymous]
    public class OfferController : Controller
    {
        private readonly IamDbContext _context;
        private readonly IConfiguration _configuration;

        public OfferController(IConfiguration configuration, ILogger<HomeController> logger, IamDbContext context)
        {
            _context = context;
            _configuration = configuration;
        }

        public IActionResult Index(int id)
        {
            var authorization = _context.Authorizations.Where(m => m.Id == id).FirstOrDefault();
            if (authorization == null)
            {
                return NotFound();
            }
            CredentialOffer credentialOffer = new CredentialOffer()
            {
                CredentialIssuer = _configuration["iss_url"] ?? string.Empty,
                Grants = new Grant()
                {
                    PreAuthorizedCode = new PreAuthorizedCode()
                }
            };
            credentialOffer.Credentials.First().Types.Add("RelationsCredential");
            credentialOffer.Grants.PreAuthorizedCode.Code = authorization.Code;
            return Content(JsonSerializer.Serialize(credentialOffer));
        }
    }
}
