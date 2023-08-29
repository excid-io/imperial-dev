using Wallet.Data;
using Wallet.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Net.Http;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Wallet.Models.Oidc4vci;
using System.Net.Http.Headers;
using System.Net;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.EntityFrameworkCore;
using Wallet.Helpers;
using System.Linq.Expressions;

namespace Wallet.Controllers
{
    public class CredentialsController : Controller
    {
        private readonly WalletDBContext _context;
        private readonly ILogger<CredentialsController> _logger;
		private string currentUser
		{
			get
            {
                return HttpContext.User.FindFirstValue(ClaimTypes.Name);
            } 
        }
        private static readonly HttpClient httpClient = new HttpClient();

        public CredentialsController(ILogger<CredentialsController> logger, WalletDBContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var credentials = _context.Credential.Where(q=>q.Owner==currentUser).ToList();
            return View(credentials);
        }

        public async Task<IActionResult> Delegate(int id)
        {
            var vc = await _context.Credential.Where(q => q.Owner == currentUser && q.Id == id).FirstOrDefaultAsync();
            if (vc == null)
            {
                return NotFound();
            }
            ViewData["vc"] = vc;
            return View(await _context.Delegations.Where(m => m.ObjectId == id && m.ObjectType ==2 && m.Owner == currentUser).ToListAsync());
        }

        public async Task<IActionResult> AddDelegation(int id)
        {
            var vc = await _context.Credential.Where(q => q.Owner == currentUser && q.Id == id).FirstOrDefaultAsync();
            if (vc == null)
            {
                return NotFound();
            }
            ViewData["vc"] = vc;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddDelegation(Delegation delegation)
        {
            var vc = await _context.Credential.Where(q => q.Owner == currentUser && q.Id == delegation.ObjectId).FirstOrDefaultAsync();
            if (vc == null)
            {
                return NotFound();
            }
            delegation.Owner = currentUser;
            delegation.ObjectType = 2;
            _context.Add(delegation);
            await _context.SaveChangesAsync();
            return Redirect(nameof(Index));
        }

        public async Task<IActionResult> Import()
        {
            var delegations = await _context.Delegations.Where(q=>q.ObjectType==2 && q.AuthType==1 && q.AuthClaim==currentUser).ToListAsync();
            return View(delegations);
        }

        [HttpPost]
        public async Task<IActionResult> Import(int delegationId)
        {
            var delegation = await _context.Delegations.Where(q => q.Id == delegationId).FirstOrDefaultAsync();
            if (delegation == null)
            {
                return NotFound();
            }
            var credential = await _context.Credential.Where(q => q.Id == delegation.ObjectId).FirstOrDefaultAsync();
            if (credential == null)
            {
                return NotFound();
            }
            var item = new Credential();
            item.type = credential.type;
            item.iss = credential.iss;
            item.jti = credential.jti;
            item.b64credential = credential.b64credential;
            item.payload = credential.payload;
            item.Owner = currentUser;
            _context.Add(item);
            _context.Delegations.Remove(delegation);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Create()
        {
            ViewData["DIDs"] = _context.DidselfDIDs.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CredentialOfferRequest request)
        {
            _logger.LogInformation("CredentialOfferRequest" + JsonSerializer.Serialize(request));
            string clientId = string.Empty;
            FormUrlEncodedContent? tokenRequest = null;
            if (request.AuthType == 1)
            {
                var did = _context.DidselfDIDs.FirstOrDefault(q => q.Owner == currentUser && q.Id == request.AuthClaimId);
                if (did != null)
                {
                    clientId = "did:self:" + did.Did;
                }
            }
            if (request.AuthType == 2)
            {
                var certificate = _context.Certificate.FirstOrDefault(q => q.Owner == currentUser && q.ID == request.AuthClaimId);
                if (certificate != null)
                {
                    clientId = certificate.Name;
                }
            }
            if (request.GrantType == "urn:ietf:params:oauth:grant-type:pre-authorized_code")
            {
                tokenRequest = new FormUrlEncodedContent(new[]
                   {
                    new KeyValuePair<string, string>("grant_type", "urn:ietf:params:oauth:grant-type:pre-authorized_code"),
                    new KeyValuePair<string, string>("pre-authorized_code", request.PreAuthorizedCode),
                    new KeyValuePair<string, string>("client_id", clientId),
                });
            }
            else if (request.GrantType == "urn:ietf:params:oauth:client-assertion-type:jwt-bearer")
            {
                string clientAssertion = string.Empty;
                if (request.AuthType == 2)
                {
                    var certificate = _context.Certificate.FirstOrDefault(q => q.Owner == currentUser && q.ID == request.AuthClaimId);
                    if (certificate != null)
                    {
                        clientAssertion = Assertion.JWTBearerAssertion(certificate, clientId, clientId, "EU.EORI.GREXCID001");
                    }
                }
                tokenRequest = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" },
                    { "scope", "Credential" },
                    { "client_id", clientId },
                    { "client_assertion_type", "urn:ietf:params:oauth:client-assertion-type:jwt-bearer" },
                    { "client_assertion", clientAssertion }
                });
            }
            if (tokenRequest == null)
            {
                _logger.LogWarning("Could not create valid token request");
                return Redirect(nameof(Index));
            }
            _logger.LogInformation("CredentialOfferRequest" + JsonSerializer.Serialize(request));
            var tokenResponse = await httpClient.PostAsync(request.IssuerURL + "/token", tokenRequest);
            TokenResponse? content;
            try
            {
                content = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>();
            }
            catch(Exception ex)
            {
                _logger.LogWarning("Did not receive token");
                return Redirect(nameof(Index));
            }
    
            if (content != null && content.access_token!=null)
            {
                _logger.LogInformation("Received token:" + content.access_token);
                //var proof = CreateJWTProof(did, issuerURL);
                var credRequest = new CredentialRequest
                {
                    format = "jwt_vc_json",
                    proof = new Proof
                    {
                        proof_type = "jwt",
                        jwt = ""//proof
                    }
                };
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", content.access_token);
                var credResponse = await httpClient.PostAsJsonAsync(request.IssuerURL + "/credential", credRequest);
                string credential = await credResponse.Content.ReadAsStringAsync();
                if (credential != "")
                {
                    _logger.LogInformation("Received credential:" + content);
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var decodedCredential = tokenHandler.ReadJwtToken(credential);
                    var item = new Credential();
                    item.type = request.CredentialType;
                    item.iss = decodedCredential.Payload["iss"].ToString()!;
                    item.jti = decodedCredential.Payload["jti"].ToString()!;
                    item.b64credential = credential;
                    item.payload = decodedCredential.Payload["vc"].ToString()!;
                    item.Owner = currentUser;
                    _context.Add(item);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    _logger.LogWarning("Did not receive credential");
                }
            }
            return Redirect(nameof(Index));

         }

        public async Task<IActionResult> Offer(String? credential_offer_uri)
        {
            var credentialOfferRequest = new CredentialOfferRequest();
            if (credential_offer_uri != null)
            {
                _logger.LogInformation("Connecting to:" + credential_offer_uri);
                var httpResponse = await httpClient.GetAsync(credential_offer_uri);
                var credentialOffer = await httpResponse.Content.ReadFromJsonAsync<CredentialOffer>();
                if (credentialOffer == null)
                {
                    return NotFound();
                }
                _logger.LogInformation("Received credential offer:" + JsonSerializer.Serialize(credentialOffer));
                if (credentialOffer.Grants != null && credentialOffer.Grants.PreAuthorizedCode != null)
                {
                    credentialOfferRequest.GrantType = "urn:ietf:params:oauth:grant-type:pre-authorized_code";
                    credentialOfferRequest.PreAuthorizedCode = credentialOffer.Grants.PreAuthorizedCode.Code;
                }
                if (credentialOffer.Grants != null && credentialOffer.Grants.JWTBarer != null)
                {
                    credentialOfferRequest.GrantType = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer";
                }
                credentialOfferRequest.CredentialType = credentialOffer.Credentials.First().Types[1];
                credentialOfferRequest.IssuerURL = credentialOffer.CredentialIssuer;
            }
            return View(credentialOfferRequest);
        }

        

        public IActionResult Authorize(AuthorizationRequest request)
        {
            ViewData["ClientName"] = "Unknown client";
            ViewData["Type"] = "Unknown type";
            string type = "";
            var scope = request.scope;
            if (scope == null)
            {
                //Add code here to handle error
            }
            //<IMPERIAL Specific code>
            if (scope == "excid-io.github.io.imperial")
            {
                ViewData["Type"] = "RelationsCredential";
                type = "RelationsCredential";
            }
            else
            {
                //Add code here to handle error
            }
            //</IMPERIAL Specific code>
            //<ISHARE Specific code>
            if (scope == "excid-io.github.io.ishare")
            {
                ViewData["Type"] = "iSHAREDelegationCredential";
                type = "iSHAREDelegationCredential";
            }
            else
            {
                //Add code here to handle error
            }
            //</ISHARE Specific code>

            if (request!.client_metadata != null)
            {
                ClientMetadata? clientMetadata =
                JsonSerializer.Deserialize<ClientMetadata>(request.client_metadata);
                if (clientMetadata?.client_name != null) {
                    ViewData["ClientName"] = clientMetadata.client_name;
                }

            }
            var credentials = _context.Credential.Where(q => q.type == type && q.Owner==currentUser).ToList();
            return View(credentials);
        }

        [HttpPost, ActionName("Authorize")]
        public async Task<IActionResult> AuthorizeConfirmed(AuthorizationRequest request, List<string>? credIds)
        {
            if (credIds == null || credIds.Count==0)
            {
                return View("Index");
            }
            _logger.LogInformation("Received authorization for: " + JsonSerializer.Serialize(credIds));

            List<string> VCs = new();
            foreach (var credId in credIds)
            {
                var VC = _context.Credential.Where(q => q.jti == credId).FirstOrDefault();
                if (VC != null)
                {
                    VCs.Add(VC.b64credential);
                }
            }
            
            var vpToken = new JwtPayload()
            {
                { "vp", new Dictionary<String, Object>()
                    {
                        {"@context", new String[]{ "https://www.w3.org/2018/credentials/v1"}},
                        {"type", new String[]{ "VerifiablePresentation" } },
                        {"verifiableCredential",VCs }
                    }
                }
            };
            var jwtHeader = new JwtHeader(
                signingCredentials: null);
            var jwtToken = new JwtSecurityToken(jwtHeader, vpToken);
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            if (request!.response_uri != null)
            {
                // https://openid.net/specs/openid-4-verifiable-presentations-1_0.html Section 6
                var authorization_response = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("state", request.state!),
                    new KeyValuePair<string, string>("vp_token", jwtTokenHandler.WriteToken(jwtToken)),
                });
                _logger.LogInformation("Ready to post:" + vpToken.Base64UrlEncode() + " to " + request.response_uri);
                var response = await httpClient.PostAsync(request.response_uri, authorization_response);
            }
            else
            {
                _logger.LogInformation("Did not post anything");
            }
            return View("AuthorizeConfirmed");
            
        }

        public IActionResult Details(int? id)
        {
            string response = string.Empty;
            //TODO add security check
            var item = _context.Credential.FirstOrDefault(q => q.Id == id);
            if (item != null)
            {
                response = item.payload;
            }
            return Ok(response);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var item = _context.Credential.Where(m => m.Id == id).FirstOrDefault();
            return View(item);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = _context.Credential.Where(m => m.Id == id).FirstOrDefault();

            if (item == null)
            {
                return NotFound();
            }
            _context.Credential.Remove(item);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
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
