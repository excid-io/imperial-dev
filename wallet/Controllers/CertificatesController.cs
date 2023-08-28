using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Wallet.Data;
using Wallet.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Wallet.Controllers
{
    public class CertificatesController : Controller
    {
        private readonly WalletDBContext _context;
        private readonly ILogger<CertificatesController> _logger;
        private string currentUser
        {
            get
            {
                return HttpContext.User.FindFirstValue(ClaimTypes.Name);
            }
        }

        public CertificatesController(ILogger<CertificatesController> logger, WalletDBContext context)
        {
            _logger = logger;
            _context = context;
        }
        
        public IActionResult Index()
        {
            var certificates = _context.Certificate.Where(q => q.Owner == currentUser).ToList();
            return View(certificates);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,X509,PrivateKey")] Certificate certificate)
        {
            if (ModelState.IsValid)
            {
                certificate.Owner = currentUser;
                _context.Add(certificate);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(certificate);
        }

        public IActionResult Edit(int id)
        {
            var did = _context.Certificate.Where(q => q.ID == id && q.Owner == currentUser).FirstOrDefault();
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
            var certificate = await _context.Certificate.FirstOrDefaultAsync(q => q.ID == id && q.Owner == currentUser);
            if (certificate == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync(
                certificate,
                "",
                s => s.Name,
                s => s.X509,
                s => s.PrivateKey))
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
            return View(certificate);
        }

        public IActionResult List()
        {
            var certificates = _context.Certificate.Where(q => q.Owner == currentUser).Select(q => new { q.ID, q.Name }).ToList();
            return Json(certificates);
        }
    }
}

