using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace CloudWallet.Models
{
    public class Credential
    {
        [Key]
        public int Id { get; set; }
        public string jti { get; set; } = "";
        public long exp { get; set; }
        public long iat { get; set; }
        public string type { get; set; } = "";
        public string payload { get; set; } = "";
        public string iss { get; set; } = "";
        public string b64credential { get; set; } = "";
        public string Owner { get; set; } = string.Empty;

        /*
        [DefaultValue(false)]
        public bool isRevoked { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int revocationIndex { get; set; }
        */
    }
}
