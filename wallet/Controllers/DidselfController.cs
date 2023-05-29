using CloudWallet.Data;
using CloudWallet.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace CloudWallet.Controllers
{
    public class DidselfController : Controller
    {
        private readonly WalletDBContext _context;

        private readonly ILogger<HomeController> _logger;

        public DidselfController(ILogger<HomeController> logger, WalletDBContext context)
        {
            _logger = logger;
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _context.DidselfDIDs.ToListAsync());
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
            var did = _context.DidselfDIDs.FirstOrDefault(q => q.Id == id);
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
        public IActionResult Manage(int id)
        {

            return View();
        }
    }
}
