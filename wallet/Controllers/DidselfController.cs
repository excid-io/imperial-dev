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
            return View(await _context.Delegations.Where(m => m.ObjectId == id && m.ObjectType == 1 &&  m.Owner == currentUser).ToListAsync());
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

        [HttpPost]
        public async Task<IActionResult> AddDelegation(Delegation delegation)
        {
            var vc = await _context.DidselfDIDs.Where(q => q.Owner == currentUser && q.Id == delegation.ObjectId).FirstOrDefaultAsync();
            if (vc == null)
            {
                return NotFound();
            }
            delegation.Owner = currentUser;
            delegation.ObjectType = 1;
            _context.Add(delegation);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
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

        public IActionResult Edit(int id)
        {
            var did = _context.DidselfDIDs.Where(q => q.Id == id && q.Owner == currentUser).FirstOrDefault();
            if (did == null)
            {
                return NotFound();
            }
            return View(did);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var did = await _context.DidselfDIDs.FirstOrDefaultAsync(q => q.Id == id && q.Owner==currentUser);
            if (did == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync(
                did,
                "",
                s => s.Name,
                s => s.AuthentiationPublicKeyJWK,
                s=>s.AuthentiationPrivateKeyJWK))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException /* ex */)
                {
                    //Log the error (uncomment ex variable name and write a log.)
                    ModelState.AddModelError("", "Unable to save changes. " +
                        "Try again, and if the problem persists, " +
                        "see your system administrator.");
                }
            }
            return View(did);
        }

        public async Task<IActionResult> Import()
        {
            var delegations = await _context.Delegations.Where(q => q.ObjectType == 1 && q.AuthType == 1 && q.AuthClaim == currentUser).ToListAsync();
            return View(delegations);
        }

        [HttpPost]
        public async Task<IActionResult> Import(int delegationId)
        {
            var delegation = await _context.Delegations.Where(q => q.Id == delegationId).FirstOrDefaultAsync();
            if (delegation == null)
            {
                return NotFound();
            }
            var did = await _context.DidselfDIDs.Where(q => q.Id == delegation.ObjectId).FirstOrDefaultAsync();
            if (did == null)
            {
                return NotFound();
            }
            var item = new Didself();
            item.Did = did.Did;
            item.Name = did.Name;
            item.Owner = currentUser;
            item.IdentifierPrivateKeyJWK = did.IdentifierPrivateKeyJWK;
            item.IdentifierPublicKeyJWK = did.AuthentiationPublicKeyJWK;
            _context.Add(item);
            _context.Delegations.Remove(delegation);
            await _context.SaveChangesAsync();
            return RedirectToAction("Edit", "Didself", new { id = item.Id });
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
            var delegation = _context.Delegations.FirstOrDefault(m => m.ObjectId == id && m.Owner == currentUser);
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
