using Wallet.Models;
using Wallet.Models.Didself;
using Microsoft.EntityFrameworkCore;


namespace Wallet.Data
{
    public class WalletDBContext : DbContext
    {
        public WalletDBContext(DbContextOptions<WalletDBContext> options)
                : base(options)
        { }
        public DbSet<Profile> Profile { get; set; }
        public DbSet<Publickey> Publickey { get; set; }
        public DbSet<Credential> Credential { get; set; }
        public DbSet<Certificate> Certificate { get; set; }
        public DbSet<Didself> DidselfDIDs { get; set; }
        public DbSet<Delegation> Delegations { get; set; }
    }
}
