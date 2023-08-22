namespace Wallet.Models.Oidc4vci
{
    public class CredentialRequest
    {
        public string format { get; set; } = string.Empty;
        public Proof proof { get; set; } = new Proof();
    }

    //Section 7.2.1 OpenIDC4VCI
    public class Proof
    {
        public string proof_type { get; set; } = string.Empty;
        public string jwt { get; set; } = string.Empty;
    }
}
