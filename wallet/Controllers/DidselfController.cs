using Wallet.Data;
using Wallet.Models.Didself;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Wallet.Models;

namespace Wallet.Controllers
{
    public class DidselfController : Controller
    {
        private readonly WalletDBContext _context;
        private readonly ILogger<DidselfController> _logger;
        private readonly IConfiguration _configuration;
        private string currentUser
		{
			get
			{
				return HttpContext.User.FindFirstValue(ClaimTypes.Name);
			}
		}

		public DidselfController(ILogger<DidselfController> logger, WalletDBContext context, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.DidselfDIDs.Where(q=>q.Owner==currentUser).ToListAsync());
        }

        public async Task<IActionResult> Delegate(int id)
        {
            var did = await _context.DidselfDIDs.Where(q => q.Owner == currentUser && q.Id == id).FirstOrDefaultAsync();
            if(did == null)
            {
                return NotFound();
            }
            ViewData["did"] = did;
            return View(await _context.Delegations.Where(m => m.DidSelfId == id && m.Owner == currentUser).ToListAsync());
        }

        public async Task<IActionResult> AddDelegation(int id)
        {
            var did = await _context.DidselfDIDs.Where(q => q.Owner == currentUser && q.Id == id).FirstOrDefaultAsync();
            if (did == null)
            {
                return NotFound();
            }
            ViewData["did"] = did;
            return View();
        }

        public IActionResult ListUsers(string pattern)
        {
            var authorizedUsers = _configuration.GetSection("AuthorizedUsers").Get<List<AuthorizedUser>>();
            var users = authorizedUsers.Where(q => q.Username.Contains(pattern)).Select(q => new { q.Username }).ToList();
            return Json(users);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AuthentiationPublicKeyJWK,AuthentiationPrivateKeyJWK,IdentifierPublicKeyJWK","IdentifierPrivateKeyJWK","Name")] Didself didself)
        {
            if (ModelState.IsValid)
            {
                var publicJWK = new JsonWebKey(didself.IdentifierPublicKeyJWK);
                var thumbprint = publicJWK.ComputeJwkThumbprint();
                didself.Did = Base64UrlEncoder.Encode(thumbprint);
                didself.Owner = currentUser;
                System.Diagnostics.Debug.WriteLine("Created:" + didself.Did);
                _context.Add(didself);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(didself);
        }
        
        public IActionResult DIDDocument(int id)
        {
            string response = string.Empty;
            //TODO add security check
            var did = _context.DidselfDIDs.FirstOrDefault(q => q.Id == id && q.Owner==currentUser);
            if (did != null)
            {
                response = $@"
{{
    ""id"": ""did:self:{did.Did}"",
    ""authentication"": [
    {{
        ""id"": ""#key1"",
        ""type"": ""JsonWebKey2020"",
        ""publicKeyJwk"": {did.AuthentiationPublicKeyJWK}
    }}
    ]
}}
                ";
            }
            return Ok(response);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var authorization = _context.DidselfDIDs.Where(m => m.Id == id && m.Owner == currentUser).FirstOrDefault();
			if (authorization == null)
			{
				return NotFound();
			}
			return View(authorization);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var resource = _context.DidselfDIDs.Where(m => m.Id == id && m.Owner == currentUser).FirstOrDefault();

            if (resource == null)
            {
                return NotFound();
            }
            _context.DidselfDIDs.Remove(resource);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult List()
        {
            var certificates = _context.DidselfDIDs.Where(q => q.Owner == currentUser).Select(q => new { q.Id, q.Name }).ToList();
            return Json(certificates);
        }

        public IActionResult Manage(int id)
        {
            var delegation = _context.Delegations.FirstOrDefault(m => m.DidSelfId == id && m.Owner == currentUser);
            @ViewData["DidSelfId"] = id;
            return View(delegation);
        }

        /*
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delegate([Bind("DidSelfId", "AuthType", "AuthClaim", "isEnabled")] Delegation delegation)
        {
            if (ModelState.IsValid)
            {
                _context.Add(delegation);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Manage));
        }*/
    }
}
