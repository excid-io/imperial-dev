using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Wallet.Models;

namespace Wallet.Helpers
{
    public class Assertion
    {
        public static string JWTBearerAssertion(Certificate certifiate, string iss, string sub, string aud)
        {
            RSA rsa;
            string[] x5c = new String[] { certifiate.X509 } ;
            rsa = RSA.Create();
            rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(certifiate.PrivateKey), out _);
            var jwtHeader = new JwtHeader(
                new SigningCredentials(
                    key: new RsaSecurityKey(rsa),
                    algorithm: SecurityAlgorithms.RsaSha256)
                )
            {
                { "x5c", x5c }
            };
            var jti = System.Guid.NewGuid();
            var iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var exp = DateTimeOffset.UtcNow.AddSeconds(30).ToUnixTimeSeconds();
            var payload = new JwtPayload
                {
                    { "jti", jti },
                    { "aud", aud },
                    { "iat", iat },
                    { "exp", exp },
                    { "iss", iss },
                    { "sub", sub }
                };

            //Create the signed token
            var jwtToken = new JwtSecurityToken(jwtHeader, payload);
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var clientAssertion = jwtTokenHandler.WriteToken(jwtToken);
            return clientAssertion;
        } 
    }
}
