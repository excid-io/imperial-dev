
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Wallet.Models;

/*
 * A simple accountcontroller used for testing authorization
 * using RBAC. Accounts and roles are provided in the configuration
 * file.
 */
namespace Wallet.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;
		private readonly ILogger<AccountController> _logger;

		public AccountController(ILogger<AccountController> logger, IConfiguration configuration)
        {
            _configuration = configuration;
			_logger = logger;
		}

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        public IActionResult ListUsers(string pattern)
        {
            var authorizedUsers = _configuration.GetSection("AuthorizedUsers").Get<List<AuthorizedUser>>();
            var users = authorizedUsers.Where(q => q.Username.Contains(pattern)).Select(q => new { q.Username }).ToList();
            return Json(users);
        }

        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string Username, string Password)
        {
            var authorizedUsers = _configuration.GetSection("AuthorizedUsers").Get<List<AuthorizedUser>>();
            var authorizedUserEntry = authorizedUsers.Where(q => q.Username == Username && q.Password == Password).FirstOrDefault();
            if (authorizedUserEntry != null)
            { 
                var claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.Name, Username));
                var roles = authorizedUserEntry.Roles;
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));
                
                return RedirectToAction("Index", "Home");
                
            }
            else
            {
                _logger.LogWarning("AccountController: User not found");
            }

            return View();
        }
    }
}
