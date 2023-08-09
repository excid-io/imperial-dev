using iam.Models.Oidc4vci;
using iam.Models.RelBAC;
using iam.Models.W3CVC;
using Microsoft.EntityFrameworkCore;


namespace iam.Data
{
    public class IamDbContext : DbContext
    {
        public IamDbContext(DbContextOptions<IamDbContext> options)
                : base(options)
        { }
        public DbSet<Authorization> Authorizations { get; set; }
        public DbSet<Token> Tokens { get; set; }
        public DbSet<Credential> Credentials { get; set; }
    }
}
