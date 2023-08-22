using System;
namespace Wallet.Models
{
	public class AuthorizedUser
	{
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
    }
}

