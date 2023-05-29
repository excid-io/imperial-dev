﻿using CloudWallet.Data;
using CloudWallet.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Net.Http;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CloudWallet.Models.Oidc4vci;
using System.Net.Http.Headers;
using System.Net;

namespace CloudWallet.Controllers
{
    public class CredentialsController : Controller
    {
        private readonly WalletDBContext _context;
        private readonly ILogger<HomeController> _logger;
        private static readonly HttpClient httpClient = new HttpClient();

        public CredentialsController(ILogger<HomeController> logger, WalletDBContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var credentials = _context.Credential.ToList();
            return View(credentials);
        }

        public IActionResult Create()
        {
            ViewData["DIDs"] = _context.DidselfDIDs.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string preAuthorizedCode, string issuerURL, int did)
        {
            var tokenRequest= new FormUrlEncodedContent(new[]
               {
                    new KeyValuePair<string, string>("grant_type", "urn:ietf:params:oauth:grant-type:pre-authorized_code"),
                    new KeyValuePair<string, string>("pre-authorized_code", preAuthorizedCode),
                });
            System.Diagnostics.Debug.WriteLine("Ready to post:" + preAuthorizedCode + " to " + issuerURL);
            var tokenResponse = await httpClient.PostAsync(issuerURL+"/token", tokenRequest);
            string content = await tokenResponse.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine("Received:" + content);
            if (content != "")
            {
                var proof = CreateJWTProof(did, issuerURL);
                System.Diagnostics.Debug.WriteLine("JWT Proof:" + proof);
                var credRequest = new CredentialRequest
                {
                    format = "jwt_vc_json",
                    proof = new Proof
                    {
                        proof_type = "jwt",
                        jwt = proof
                    }
                };
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", content);
                var credResponse = await httpClient.PostAsJsonAsync(issuerURL + "/credential", credRequest);
                string credential = await credResponse.Content.ReadAsStringAsync();
                if (credential != "")
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var decodedCredential = tokenHandler.ReadJwtToken(credential);
                    var item = new Credential();
                    item.type = "RelationsCredential";
                    item.iss = decodedCredential.Payload["iss"].ToString()!;
                    item.jti = decodedCredential.Payload["jti"].ToString()!;
                    item.b64credential = credential;
                    item.payload = decodedCredential.Payload["vc"].ToString()!;
                    _context.Add(item);
                    await _context.SaveChangesAsync();
                }
            }
            return Redirect(nameof(Index));

        }

        public IActionResult Authorize(AuthorizationRequest request)
        {
            ViewData["ClientName"] = "Unknown client";
            ViewData["Type"] = "Unknown type";

            var scope = request.scope;
            if (scope == null)
            {
                //Add code here to handle error
            }
            //<IMPERIAL Specific code>
            if (scope == "excid-io.github.io.imperial")
            {
                ViewData["Type"] = "RelationsCredential";
            }
            else
            {
                //Add code here to handle error
            }
            //</IMPERIAL Specific code>
            
            if (request!.client_metadata != null)
            {
                ClientMetadata? clientMetadata =
                JsonSerializer.Deserialize<ClientMetadata>(request.client_metadata);
                if (clientMetadata?.client_name != null) {
                    ViewData["ClientName"] = clientMetadata.client_name;
                }

            }
            var credentials = _context.Credential.Where(q => q.type == "RelationsCredential").ToList();
            return View(credentials);
        }

        [HttpPost, ActionName("Authorize")]
        public async Task<IActionResult> AuthorizeConfirmed(AuthorizationRequest request, string cred_id="")
        {
            System.Diagnostics.Debug.WriteLine("Received authorization for: " + cred_id);
            var _credential = _context.Credential.Where(q => q.jti == cred_id).FirstOrDefault();
            if (_credential != null && request!.response_uri != null)
            {
                // https://openid.net/specs/openid-4-verifiable-presentations-1_0.html Section 6
                var authorization_response = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("state", request.state!),
                    new KeyValuePair<string, string>("vp_token", _credential.b64credential),
                });
                System.Diagnostics.Debug.WriteLine("Ready to post:" + _credential.jti + " to " + request.response_uri);
                var response = await httpClient.PostAsync(request.response_uri, authorization_response);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Did not post anything");
            }
            return View("AuthorizeConfirmed");
        }

        public async Task<IActionResult> Initialize()
        {
            var credential = new Credential();
            credential.jti = "credential:2";
            credential.payload = "eyJhbGciOiJFUzI1NiIsImp3ayI6eyJjcnYiOiJQLTI1NiIsImt0eSI6IkVDIiwieCI6InozMFd1eHBzUG93OEtwSDBOOTN2VzI0bkEwSEQ0OF9NbHVxZ2RFVXZ0VTQiLCJ5IjoiVmNLY28xMkJaRlB1NUhVMkxCTG90VEQ5Tml0ZGxOeG5CTG5nRC1lVGFwTSJ9LCJ0eXAiOiJqd3QifQ.eyJhdWQiOiJodHRwczovL2dhdGV3YXkuZXhjaWQuaW8iLCJpc3MiOiJodHRwOi8vdGVzdHNjcmlwdCIsInN1YiI6ImRpZDpzZWxmOnBPNWxsTlBqXy1wWWlVZmdqMDY2ZzZBaTJTMmU4SGkzMl9wU24tZjBpNkEiLCJ2YyI6eyJAY29udGV4dCI6WyJodHRwczovL3d3dy53My5vcmcvMjAxOC9jcmVkZW50aWFscy92MSIsImh0dHBzOi8vZXhjaWQtaW8uZ2l0aHViLmlvL2ltcGVyaWFsL2NvbnRleHRzL3YxIl0sImNyZWRlbnRpYWxTdWJqZWN0Ijp7InJlbGF0aW9uc2hpcHMiOlt7Im9iamVjdCI6ImhvbWUiLCJyZWxhdGlvbiI6InJlYWQifSx7Im9iamVjdCI6ImhvbWUiLCJyZWxhdGlvbiI6IndyaXRlIn1dfSwidHlwZSI6WyJWZXJpZmlhYmxlQ3JlZGVudGlhbCIsIlJlbGF0aW9uc0NyZWRlbnRpYWwiXX19.Ck0IhUyMyKdpT90lC1DIeOuMx8-xFgR4zYxCvDvxAuYBSwP15s5jR4ZQsknbejHog_02Ootw_zzXQrvxQdrSzQ";
            credential.type = "RelationsCredential";
            _context.Add(credential);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private string CreateJWTProof(int id, string issuer)
        {
            var did = _context.DidselfDIDs.FirstOrDefault(q => q.Id == id);
            if (did == null)
            {
                return string.Empty;
            }
            var iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var signingJWK = new JsonWebKey(did.IdentifierPrivateKeyJWK);
            var publicJWK = new JsonWebKey(did.IdentifierPrivateKeyJWK);
            var jwtHeader = new JwtHeader(
                new SigningCredentials(
                    key: signingJWK,
                    algorithm: SecurityAlgorithms.EcdsaSha256)
                )
            {
                { "kid", "did:self:" + did.Did }
            };
            var payload = new JwtPayload()
            {
                { "iat", iat },
                { "aud", issuer },
                { "nonce", "c_nonce" },
            };
            var jwtToken = new JwtSecurityToken(jwtHeader, payload);
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            return jwtTokenHandler.WriteToken(jwtToken);
        }

    }
}
