using CloudWallet.Models;
using CloudWallet.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CloudWallet.Controllers
{
    public class ProfileController : Controller
    {
        private readonly WalletDBContext _context;

        private readonly ILogger<HomeController> _logger;

        public ProfileController(ILogger<HomeController> logger, WalletDBContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Profile.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProfileName,PublicKeyJWK,PrivateKey")] Profile profile)
        {
            if (ModelState.IsValid)
            {
                _context.Add(profile);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(profile);
        }

        public IActionResult Sign()
        {
            Profile profile = _context.Profile.First(q => q.ProfileName == "profile-1");
            return View(profile);
        }
    }
}
