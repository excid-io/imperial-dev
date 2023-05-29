using iam.Models.Oidc4vci;
using iam.Models.RelBAC;
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
    }
}
