using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Wallet.Controllers;
using Wallet.Data;

namespace Wallet.Models
{
    public class Delegation
    {
        [NotMapped]
        private readonly WalletDBContext? _context;

        [Key]
        public int Id { get; set; }
        public int ObjectType { get; set; } //1=DID 2=VC
        public int ObjectId { get; set; }
        [DisplayName("Delegate to")]
        public int AuthType { get; set; } //1=Group 2=User
        public string AuthClaim { get; set; } = string.Empty;
        public bool isEnabled { get; set; } = true;
		public string Owner { get; set; } = string.Empty;

        [NotMapped]
        public virtual string CredentialType
        {
            get
            {
                if (_context != null)
                {
                    return _context.Credential.Where(q => q.Id == ObjectId).First().type;
                }
                else
                {
                    return "";
                }
            }
        }

        [NotMapped]
        public virtual string DIDName
        {
            get
            {
                if (_context != null)
                {
                    return _context.DidselfDIDs.Where(q => q.Id == ObjectId).First().Name;
                }
                else
                {
                    return "";
                }
            }
        }

        [NotMapped]
        public static readonly List<SelectListItem> AuthTypes = new List<SelectListItem>()
        {
            new SelectListItem {Text = "Group", Value = "0"},
            new SelectListItem {Text = "User", Value = "1"},
        };

        public Delegation(WalletDBContext context)
        {
            _context = context;
        }

        public Delegation()
        {
            
        }

    }
}
