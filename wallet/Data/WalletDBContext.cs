using CloudWallet.Models;
using Microsoft.EntityFrameworkCore;


namespace CloudWallet.Data
{
    public class WalletDBContext : DbContext
    {
        public WalletDBContext(DbContextOptions<WalletDBContext> options)
                : base(options)
        { }
        public DbSet<Profile> Profile { get; set; }
        public DbSet<Publickey> Publickey { get; set; }
        public DbSet<Credential> Credential { get; set; }
        public DbSet<Didself> DidselfDIDs { get; set; }
    }
}
