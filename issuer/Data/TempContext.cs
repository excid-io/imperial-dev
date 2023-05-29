using iam.Models.Oidc4vci;

namespace iam.Data
{
    public class TempContext
    {
        public static List<Token> Tokens { get; set; } = new List<Token>();

    }
}
