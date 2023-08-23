using iam.Data;
using iam.Models.Role;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Resources;
using System.Security.Cryptography;
using System.Text.Json;

namespace iam.Controllers
{
    public class RoleController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IamDbContext _context;
        private readonly ILogger<RoleController> _logger;

        public RoleController(IConfiguration configuration, ILogger<RoleController> logger, IamDbContext context)
        {
            _configuration = configuration;
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            ViewData["wallet_url"] = _configuration["wallet_url"];
            ViewData["iss_url"] = _configuration["iss_url"];
            var authorizations = _context.Authorizations.ToList();
            return View(authorizations);
        }

        public IActionResult Create()
        {
            var resources = _configuration.GetSection("Roles").Get<List<String>>();
            ViewData["relObjects"] = resources;
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name", "AuthType", "AvailableRoles")] Models.Role.Authorization authorization)
        {
            List<Dictionary<string, string>> roles = new ();
            foreach (var role in authorization.AvailableRoles)
            {
                if (role != "false")
                    roles.Add(new Dictionary<string, string>() { { "ACME", role } });
            }  

            authorization.Roles = JsonSerializer.Serialize(roles);
            authorization.Code = RandomString();
            _logger.LogInformation("Added new assignment:" + JsonSerializer.Serialize(authorization));
            _context.Add(authorization);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var authorization = _context.Authorizations.Where(m => m.Id == id).FirstOrDefault();
            return View(authorization);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var resource = _context.Authorizations.Where(m => m.Id == id).FirstOrDefault();

            if (resource == null)
            {
                return NotFound();
            }
            _context.Authorizations.Remove(resource);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Status(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var authorization = _context.Authorizations.Where(m => m.Id == id).FirstOrDefault();
            return View(authorization);
        }


        [HttpPost, ActionName("Status")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StatusConfirmed(int id)
        {
            var resource = _context.Authorizations.Where(m => m.Id == id).FirstOrDefault();

            if (resource == null)
            {
                return NotFound();
            }
            resource.Revoked = !resource.Revoked;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Details(int? id)
        {
            string response = string.Empty;
            //TODO add security check
            var authorization = _context.Authorizations.FirstOrDefault(q => q.Id == id);
            if (authorization != null)
            {
                response = authorization.Roles;
            }
            return Ok(response);
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
