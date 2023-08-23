
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

/*
 * A simple accountcontroller used for testing authorization
 * using RBAC. Accounts and roles are provided in the configuration
 * file.
 */
namespace iam.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;

        public AccountController( IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
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
            var authorizedUserEntry = _configuration.GetSection("AuthorizedUsers").GetSection(Username);
            if (authorizedUserEntry.Exists())
            {
                if (authorizedUserEntry["Password"] != Password)
                {
                    ViewData["ErrorDscr"] = "Incorrect password";
                }
                else
                {
                    var claims = new List<Claim>();
                    claims.Add(new Claim(ClaimTypes.Name, Username));
                    var roles = authorizedUserEntry.GetSection("Roles").Get<string[]>();
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
            }
            else
            {
                ViewData["ErrorDscr"] = "User not found";
            }

            return View();
        }
    }
}
