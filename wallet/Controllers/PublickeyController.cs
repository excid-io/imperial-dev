using CloudWallet.Models;
using CloudWallet.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CloudWallet.Controllers
{
    public class PublickeyController : Controller
    {
        private readonly WalletDBContext _context;

        private readonly ILogger<HomeController> _logger;

        public PublickeyController(ILogger<HomeController> logger, WalletDBContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Publickey.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,PublicKeyJWK,PrivateKey")] Publickey publickey)
        {
            if (ModelState.IsValid)
            {
                _context.Add(publickey);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(publickey);
        }

        public IActionResult Sign()
        {
            return View();
        }

        public async Task<string> getPublicKeyJWT([FromBody] PublicKeyJWKRequest request)
        {
            var publickey = await _context.Publickey.FirstOrDefaultAsync(q => q.Name == request.Name);
            if (publickey == null)
            {
                return "{ \"result\" : \"Error\"}";
            }
            else
            {
                return $"{{ \"result\" : \"OK\", \"key\": {publickey.PublicKeyJWK} }}";
            }
        }
    }
}
