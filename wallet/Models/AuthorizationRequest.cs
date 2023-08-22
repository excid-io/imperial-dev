namespace Wallet.Models
{
    public class AuthorizationRequest
    {
        public string? scope { get; set; }
        public string? nonce { get; set; }
        public string? response_type { get; set; }
        public string? response_mode { get; set; }
        public string? response_uri { get; set; }
        public string? state { get; set; }
        public string? client_metadata { get; set; }
    }

    public class ClientMetadata
    {
        public string? client_name { get; set; }
    }
}
