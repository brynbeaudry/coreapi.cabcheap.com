using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using api.cabcheap.com.Models;
using api.cabcheap.com.Models.AccountViewModels;
using api.cabcheap.com.Services;
using Microsoft.Extensions.Configuration;
using api.cabcheap.com.Data;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net;
using Newtonsoft.Json.Linq;
using AspNet.Security.OpenIdConnect.Primitives;
using AspNet.Security.OpenIdConnect.Server;
using AspNet.Security.OpenIdConnect.Extensions;
using OpenIddict.Core;
using AspNet.Security.OAuth.Validation;
using OpenIddict.Models;
using System.Threading;
using System.Collections.Immutable;

namespace api.cabcheap.com.Controllers
{
    public class AuthorizationController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
            private readonly SignInManager<ApplicationUser> _signInManager;
            private readonly IEmailSender _emailSender;
            private readonly ILogger _logger;
            private readonly IConfiguration _config;

            private readonly OpenIddictTokenManager<OpenIddictToken> _tokenManager;

            private readonly OpenIddictAuthorizationManager<OpenIddictAuthorization> _authorizationManager;



            private readonly ApplicationDbContext _ctx;
            private const string GoogleApiTokenInfoUrl = "https://www.googleapis.com/oauth2/v3/tokeninfo?id_token={0}";
            private const string GoogleApiUserInfoUrl = "https://www.googleapis.com/plus/v1/people/{0}?userIp={1}&key={2}";
            private const string FacebookApiUserInfoUrl = "https://graph.facebook.com/{2}?input_token={0}&access_token={1}&fields={3}";
            private const string FacebookApiTokenInfoUrl = "https://graph.facebook.com/debug_token?input_token={0}&access_token={1}";
            

            public AuthorizationController(
                UserManager<ApplicationUser> userManager,
                SignInManager<ApplicationUser> signInManager,
                IEmailSender emailSender,
                ILogger<AuthorizationController> logger,
                IConfiguration configuration,
                ApplicationDbContext ctx,
                OpenIddictTokenManager<OpenIddictToken> tokenManager,
                OpenIddictAuthorizationManager<OpenIddictAuthorization> authorizationManager

                )
            {
                _userManager = userManager;
                _signInManager = signInManager;
                _emailSender = emailSender;
                _logger = logger;
                _config = configuration;
                _ctx = ctx;
                _tokenManager = tokenManager;
                _authorizationManager = authorizationManager;
            }

        public async Task<ProviderUserDetails> GetGoogleDetailsAsync(string providerToken)
        {

            var httpClient = new HttpClient();
            var requestUri = new Uri(string.Format(GoogleApiTokenInfoUrl, providerToken));

            var tokenInfoResponse = await getHttpResponseResult(GoogleApiTokenInfoUrl, providerToken, "GOOGLE");

            GoogleApiTokenInfo googleApiTokenInfo = JsonConvert.DeserializeObject<GoogleApiTokenInfo>(tokenInfoResponse.ToString());

            var googleUserInfoResponse = await getHttpResponseResult(GoogleApiUserInfoUrl, providerToken, "GOOGLE", googleApiTokenInfo.sub);
            GooglePersonInfo googleUserInfo = JsonConvert.DeserializeObject<GooglePersonInfo>(googleUserInfoResponse);

            /* if (!SupportedClientsIds.Contains(googleApiTokenInfo.aud))
            {
                Log.WarnFormat("Google API Token Info aud field ({0}) not containing the required client id", googleApiTokenInfo.aud);
                return null;
            } */

            return new ProviderUserDetails
            {
                Email = googleApiTokenInfo.email ?? "NONE",
                FirstName = googleApiTokenInfo.given_name ?? googleUserInfo.name["givenName"] ?? "NOT AVAILABLE",
                LastName = googleApiTokenInfo.family_name ?? googleUserInfo.name["familyName"] ?? "NOT AVAILABLE",
                Locale = googleApiTokenInfo.locale ?? "EN-us",
                Name = googleApiTokenInfo.name ?? $"{googleUserInfo.name["givenName"]} {googleUserInfo.name["familyName"]}",
                ProviderUserId = googleApiTokenInfo.sub ?? googleUserInfo.id
            };
        }

        public async Task<string> getHttpResponseResult(string url, string providerToken, string providerName, string ProviderUserId=""){
            var httpClient = new HttpClient();
            Uri RequestUri;
            // new Uri(string.Format(GoogleApiTokenInfoUrl, providerToken));
            if(providerName == "FACEBOOK")
            {
                var FacebookConfig = _config.GetSection("ExternalIdentities").GetSection("Facebook");
                var AppId = FacebookConfig["app_id"];
                var AppSecret = FacebookConfig["app_secret"];
                if(ProviderUserId.CompareTo("") == 0){
                    //facebook id is nothing
                    RequestUri = new Uri(string.Format(url, providerToken, $"{AppId}|{AppSecret}"));
                }else{
                    //have provider user id , get user id
                    RequestUri = new Uri(string.Format(url, providerToken, $"{AppId}|{AppSecret}", ProviderUserId, "id,email,name,about,first_name,last_name,locale"));
                }
            }
            else if(providerName == "GOOGLE"){
                var GoogleConfig = _config.GetSection("ExternalIdentities").GetSection("Google");
                var GoogleApiKey = GoogleConfig["api_key"];

                /* private const string GoogleApiTokenInfoUrl = "https://www.googleapis.com/oauth2/v3/tokeninfo?id_token={0}";
                private const string GoogleApiUserInfoUrl = "https://www.googleapis.com/plus/v1/people/{0}?userIp={1}&key={2}"; */

                if(ProviderUserId.CompareTo("") == 0){
                    //fprovider id is nothing, get token info
                    RequestUri = new Uri(string.Format(url, providerToken));
                }else{
                    //have provider user id , get user info
                    RequestUri = new Uri(string.Format(url, ProviderUserId, providerToken, GoogleApiKey));
                }
            }
            else
            {
                return "NEITHER GOOGLE NOR FACEBOOK";
            }
            
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
            httpResponseMessage = await httpClient.GetAsync(RequestUri);

            if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
            {
                //Console.WriteLine(ex.Message);
                return null;
            }

            var response = httpResponseMessage.Content.ReadAsStringAsync().Result;
            return response;
        }

        public async Task<ProviderUserDetails> GetFacebookDetailsAsync(string providerToken)
        {
            

            var tokenInfoResponse = await getHttpResponseResult(FacebookApiTokenInfoUrl, providerToken, "FACEBOOK");
            var responseDataWrapper = JObject.Parse(tokenInfoResponse);
            //the actual memebers of interest are wrapped in data key
            var unwrappedResponse = responseDataWrapper.GetValue("data");

            FacebookApiTokenInfo facebookApiTokenInfo = JsonConvert.DeserializeObject<FacebookApiTokenInfo>(unwrappedResponse.ToString());
            
            var fbUserInfoResponse = await getHttpResponseResult(FacebookApiUserInfoUrl, providerToken, "FACEBOOK", facebookApiTokenInfo.user_id);
            var fbUserInfo = JsonConvert.DeserializeObject<FacebookApiUserInfo>(fbUserInfoResponse);

            /* if (!SupportedClientsIds.Contains(googleApiTokenInfo.aud))
            {
                Log.WarnFormat("Google API Token Info aud field ({0}) not containing the required client id", googleApiTokenInfo.aud);
                return null;
            } */

            return new ProviderUserDetails
            {
                Email = fbUserInfo.email,
                FirstName = fbUserInfo.first_name,
                LastName = fbUserInfo.last_name,
                Locale = fbUserInfo.locale,
                Name = fbUserInfo.name,
                ProviderUserId = fbUserInfo.id
            };
        }
        
        [HttpPost("/connect/token"), Produces("application/json")]
        public async Task<IActionResult> ExchangeAsync(OpenIdConnectRequest request)
        {
            // Reject the token request if client_id or client_secret is missing.
                /* if (string.IsNullOrEmpty(request.ClientId) || string.IsNullOrEmpty(request.ClientSecret))
                {
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidRequest,
                        ErrorDescription = "Missing credentials: ensure that your credentials were correctly " +
                                        "flowed in the request body or in the authorization header"
                    });
                } */

                // Retrieve the application details from the database.
                /* var application = await _applicationManager.FindByClientIdAsync(request.ClientId, HttpContext.RequestAborted);

                if (application == null)
                {
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidClient,
                        ErrorDescription = "The credentials is invalid."
                    });
                } */

                if (request.IsPasswordGrantType())
                {
                    /* if (!string.Equals(application.Type, OpenIddictConstants.ClientTypes.Confidential, StringComparison.Ordinal))
                    {
                        return BadRequest(new OpenIdConnectResponse
                        {
                            Error = OpenIdConnectConstants.Errors.InvalidClient,
                            ErrorDescription = "Only confidential clients are allowed to use password grant type."
                        });
                    } */
                    var user = _ctx.Users
                        .Where(x=> x.Email.Equals(request.Username))
                        .Where(x => x.ProviderName.Equals("EMAIL"))
                        .FirstOrDefault();
                    //var user = await _userManager.FindByEmailAsync(request.Username);

                    if (user == null)
                    {
                        return BadRequest(new OpenIdConnectResponse
                        {
                            Error = OpenIdConnectConstants.Errors.InvalidGrant,
                            ErrorDescription = "The username/password couple is invalid."
                        });
                    }
                    var canSignIn = await _signInManager.CanSignInAsync(user);
                    // Ensure the user is allowed to sign in.
                    if (!canSignIn)
                    {
                        return BadRequest(new OpenIdConnectResponse
                        {
                            Error = OpenIdConnectConstants.Errors.InvalidGrant,
                            ErrorDescription = "The specified user is not allowed to sign in."
                        });
                    }

                    // Reject the token request if two-factor authentication has been enabled by the user.
                    if (_userManager.SupportsUserTwoFactor && await _userManager.GetTwoFactorEnabledAsync(user))
                    {
                        return BadRequest(new OpenIdConnectResponse
                        {
                            Error = OpenIdConnectConstants.Errors.InvalidGrant,
                            ErrorDescription = "The specified user is not allowed to sign in."
                        });
                    }

                    // Ensure the user is not already locked out.
                    if (_userManager.SupportsUserLockout && await _userManager.IsLockedOutAsync(user))
                    {
                        return BadRequest(new OpenIdConnectResponse
                        {
                            Error = OpenIdConnectConstants.Errors.InvalidGrant,
                            ErrorDescription = "The username/password couple is invalid."
                        });
                    }

                    // Ensure the password is valid.
                    if (!await _userManager.CheckPasswordAsync(user, request.Password))
                    {
                        if (_userManager.SupportsUserLockout)
                        {
                            await _userManager.AccessFailedAsync(user);
                        }

                        return BadRequest(new OpenIdConnectResponse
                        {
                            Error = OpenIdConnectConstants.Errors.InvalidGrant,
                            ErrorDescription = "The username/password couple is invalid."
                        });
                    }

                    if (_userManager.SupportsUserLockout)
                    {
                        await _userManager.ResetAccessFailedCountAsync(user);
                    }
                    
                    var identity = new ClaimsIdentity(OpenIdConnectServerDefaults.AuthenticationScheme);

                    identity.AddClaim(OpenIdConnectConstants.Claims.Subject,
                        user.Id,
                            OpenIdConnectConstants.Destinations.AccessToken);
                    identity.AddClaim(OpenIdConnectConstants.Claims.Email, 
                        user.Email, 
                            OpenIdConnectConstants.Destinations.AccessToken);
                    if(user.FirstName!=null){
                    identity.AddClaim(OpenIdConnectConstants.Claims.GivenName, 
                        user.FirstName, 
                            OpenIdConnectConstants.Destinations.AccessToken);
                    }
                    if(user.LastName!=null){
                    identity.AddClaim(OpenIdConnectConstants.Claims.FamilyName, 
                        user.LastName, 
                            OpenIdConnectConstants.Destinations.AccessToken);
                    }

                    var ticket = new AuthenticationTicket(
                        new ClaimsPrincipal(identity),
                        new AuthenticationProperties(),
                        OpenIdConnectServerDefaults.AuthenticationScheme);
                    
                    ticket.SetScopes(
                        OpenIdConnectConstants.Scopes.OpenId,
                        OpenIdConnectConstants.Scopes.OfflineAccess, 
                        OpenIdConnectConstants.Scopes.Profile, 
                        OpenIdConnectConstants.Scopes.Email);



                    return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
                }

                if (request.IsAuthorizationCodeGrantType())
                {
                    // Retrieve the claims principal stored in the authorization code.
                    var info = await HttpContext.AuthenticateAsync(
                        OpenIdConnectServerDefaults.AuthenticationScheme);

                    // Retrieve the user profile corresponding to the authorization code.
                    // Note: if you want to automatically invalidate the authorization code
                    // when the user password/roles change, use the following line instead:
                    //var user = await _signInManager.ValidateSecurityStampAsync(info.Principal);
                    var user = await _userManager.GetUserAsync(info.Principal);

                    if (user == null)
                    {
                        return BadRequest(new OpenIdConnectResponse
                        {
                            Error = OpenIdConnectConstants.Errors.InvalidGrant,
                            ErrorDescription = "The authorization code is no longer valid."
                        });
                    }

                    // Ensure the user is still allowed to sign in.
                    if (!await _signInManager.CanSignInAsync(user))
                    {
                        return BadRequest(new OpenIdConnectResponse
                        {
                            Error = OpenIdConnectConstants.Errors.InvalidGrant,
                            ErrorDescription = "The user is no longer allowed to sign in."
                        });
                    }

                    // Create a new authentication ticket, but reuse the properties stored
                    // in the authorization code, including the scopes originally granted.
                    var ticket = await CreateTicketAsync(request, user, info.Properties);

                    return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
                }   

                if (request.IsRefreshTokenGrantType())
                {
                    // Retrieve the claims principal stored in the refresh token.
                    var info = await HttpContext.AuthenticateAsync(
                        OpenIdConnectServerDefaults.AuthenticationScheme);

                    // Retrieve the user profile corresponding to the refresh token.
                    // Note: if you want to automatically invalidate the refresh token
                    // when the user password/roles change, use the following line instead:
                    var user = await _signInManager.ValidateSecurityStampAsync(info.Principal);
                    //var user = await _userManager.GetUserAsync(info.Principal);

                    if (user == null)
                    {
                        return BadRequest(new OpenIdConnectResponse
                        {
                            Error = OpenIdConnectConstants.Errors.InvalidGrant,
                            ErrorDescription = "The refresh token is no longer valid."
                        });
                    }

                    // Ensure the user is still allowed to sign in.
                    if (!await _signInManager.CanSignInAsync(user))
                    {
                        return BadRequest(new OpenIdConnectResponse
                        {
                            Error = OpenIdConnectConstants.Errors.InvalidGrant,
                            ErrorDescription = "The user is no longer allowed to sign in."
                        });
                    }

                    // Create a new authentication ticket, but reuse the properties stored
                    // in the refresh token, including the scopes originally granted.
                    var ticket = await CreateTicketAsync(request, user, info.Properties);

                    return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
                }

            if (request.GrantType == "urn:ietf:params:oauth:grant-type:google_identity_token")
            {
                // Reject the request if the "assertion" parameter is missing.
                if (string.IsNullOrEmpty(request.Assertion))
                {
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidRequest,
                        ErrorDescription = "The mandatory 'assertion' parameter was missing."
                    });
                }


                // Create a new ClaimsIdentity containing the claims that
                // will be used to create an id_token and/or an access token.
                var identity = new ClaimsIdentity(OpenIdConnectServerDefaults.AuthenticationScheme);
                Console.WriteLine(identity.ToString());
                Console.WriteLine(request.Code);

                var googleDetails = await GetGoogleDetailsAsync(request.Assertion);
                if(googleDetails != null){
                // save 
                    //check to see if the ProviderId is already listed for this user
                    var repeatUser = _ctx.Users
                        .Where(x => x.ProviderId.Equals(googleDetails.ProviderUserId))
                        .FirstOrDefault();
                    
                    //this signed in google user is not a repeat user and hasn't been saved yet
                    if(repeatUser == null)
                    {

                        var result = await _userManager.CreateAsync(
                        new ApplicationUser(){ 
                            Email = googleDetails.Email,
                            FirstName = googleDetails.FirstName, 
                            UserName = $"{googleDetails.Email.Split('@')[0]}_Google",
                            FullName = googleDetails.Name,
                            LastName = googleDetails.LastName,
                            ProviderId = googleDetails.ProviderUserId,
                            ProviderName = "GOOGLE",
                            PictureUrl = "https://cdn.iconscout.com/public/images/icon/premium/png-512/gamer-games-video-casino-372bcf114ef0140a-512x512.png"
                        });
                        if (!result.Succeeded)
                        {
                            AddErrors(result);
                            return BadRequest(result);
                        }else{
                            _logger.LogInformation("Social User created a new account without password.");

                            //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                            //var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
                            //await _emailSender.SendEmailConfirmationAsync(model.Email, callbackUrl);

                            //await _signInManager.SignInAsync(user, isPersistent: false);
                            //_logger.LogInformation("User created a new account with password.");
                            //return Ok(result);
                            //return RedirectToLocal(returnUrl);
                        }
                    }
                }


                // Manually validate the identity token issued by Google,
                // including the issuer, the signature and the audience.
                // Then, copy the claims you need to the "identity" instance.
                identity.AddClaim(OpenIdConnectConstants.Claims.Subject,
                        googleDetails.ProviderUserId,
                        OpenIdConnectConstants.Destinations.AccessToken);
                identity.AddClaim(OpenIdConnectConstants.Claims.Name, googleDetails.Name,
                        OpenIdConnectConstants.Destinations.AccessToken);
                identity.AddClaim(OpenIdConnectConstants.Claims.Email, 
                    googleDetails.Email, 
                        OpenIdConnectConstants.Destinations.AccessToken);
                identity.AddClaim(OpenIdConnectConstants.Claims.GivenName, 
                    googleDetails.FirstName, 
                        OpenIdConnectConstants.Destinations.AccessToken);
                identity.AddClaim(OpenIdConnectConstants.Claims.FamilyName, 
                    googleDetails.LastName, 
                        OpenIdConnectConstants.Destinations.AccessToken);
                identity.AddClaim(OpenIdConnectConstants.Claims.Locale, 
                    googleDetails.Locale, 
                        OpenIdConnectConstants.Destinations.AccessToken);

                // Create a new authentication ticket holding the user identity.
                

                var ticket = new AuthenticationTicket(
                    new ClaimsPrincipal(identity),
                    new AuthenticationProperties(),
                    OpenIdConnectServerDefaults.AuthenticationScheme);

                // Then, copy the claims you need to the "identity" instance.

                ticket.SetScopes(
                            OpenIdConnectConstants.Scopes.OpenId,
                            OpenIdConnectConstants.Scopes.OfflineAccess, 
                            OpenIdConnectConstants.Scopes.Profile, 
                            OpenIdConnectConstants.Scopes.Email);

            return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
        }
        
        if (request.GrantType == "urn:ietf:params:oauth:grant-type:facebook_access_token")
        {
            // Reject the request if the "assertion" parameter is missing.
            if (string.IsNullOrEmpty(request.Assertion))
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.InvalidRequest,
                    ErrorDescription = "The mandatory 'assertion' parameter was missing."
                });
            }

            // Create a new ClaimsIdentity containing the claims that
            // will be used to create an id_token and/or an access token.
            var identity = new ClaimsIdentity(OpenIdConnectServerDefaults.AuthenticationScheme);
            Console.WriteLine(identity.ToString());
            Console.WriteLine(request.Code);

            var fbDetails = await GetFacebookDetailsAsync(request.Assertion);
            if(fbDetails != null){
                    // save 
                    //check to see if the ProviderId is already listed for this user
                    var repeatUser = _ctx.Users
                        .Where(x => x.ProviderId.Equals(fbDetails.ProviderUserId))
                        .FirstOrDefault();
                // save 
                if(repeatUser == null)
                    {
                        var result = await _userManager.CreateAsync(
                            new ApplicationUser(){ 
                                Email = fbDetails.Email, 
                                UserName = $"{fbDetails.Email.Split('@')[0]}_Facebook",
                                FirstName = fbDetails.FirstName,
                                FullName = fbDetails.Name,
                                LastName = fbDetails.LastName,
                                ProviderId = fbDetails.ProviderUserId,
                                ProviderName = "FACEBOOK",
                                PictureUrl = "https://cdn.iconscout.com/public/images/icon/premium/png-512/gamer-games-video-casino-372bcf114ef0140a-512x512.png"
                        });
                        if (!result.Succeeded)
                        {
                            AddErrors(result);
                            return BadRequest(result);
                        }else{
                            _logger.LogInformation("Social User created a new account without password.");

                            //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                            //var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
                            //await _emailSender.SendEmailConfirmationAsync(model.Email, callbackUrl);

                            //await _signInManager.SignInAsync(user, isPersistent: false);
                            //_logger.LogInformation("User created a new account with password.");
                            //return Ok(result);
                            //return RedirectToLocal(returnUrl);
                        }
                    }
                }

                // Manually validate the identity token issued by Google,
                // including the issuer, the signature and the audience.
                // Then, copy the claims you need to the "identity" instance.
                identity.AddClaim(OpenIdConnectConstants.Claims.Subject,
                        fbDetails.ProviderUserId,
                        OpenIdConnectConstants.Destinations.AccessToken);
                identity.AddClaim(OpenIdConnectConstants.Claims.Name, fbDetails.Name,
                        OpenIdConnectConstants.Destinations.AccessToken);
                identity.AddClaim(OpenIdConnectConstants.Claims.Email, 
                    fbDetails.Email, 
                        OpenIdConnectConstants.Destinations.AccessToken);
                identity.AddClaim(OpenIdConnectConstants.Claims.GivenName, 
                    fbDetails.FirstName, 
                        OpenIdConnectConstants.Destinations.AccessToken);
                identity.AddClaim(OpenIdConnectConstants.Claims.FamilyName, 
                    fbDetails.LastName, 
                        OpenIdConnectConstants.Destinations.AccessToken);
                identity.AddClaim(OpenIdConnectConstants.Claims.Locale, 
                    fbDetails.Locale, 
                        OpenIdConnectConstants.Destinations.AccessToken);

                // Create a new authentication ticket holding the user identity.
                

                var ticket = new AuthenticationTicket(
                    new ClaimsPrincipal(identity),
                    new AuthenticationProperties(),
                    OpenIdConnectServerDefaults.AuthenticationScheme);

                // Then, copy the claims you need to the "identity" instance.

                ticket.SetScopes(
                            OpenIdConnectConstants.Scopes.OpenId,
                            OpenIdConnectConstants.Scopes.OfflineAccess, 
                            OpenIdConnectConstants.Scopes.Profile, 
                            OpenIdConnectConstants.Scopes.Email);

                return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
            }

            return BadRequest(new OpenIdConnectResponse
            {
                Error = OpenIdConnectConstants.Errors.UnsupportedGrantType,
                ErrorDescription = "The specified grant type is not supported."
            });

                
        }

            //[Authorize]
            //[ValidateAntiForgeryToken]
            //[FormValueRequired("Authorize")]
            [HttpPost("connect/authorize")]
            public async Task<IActionResult> Accept(OpenIdConnectRequest request)
            {
                // Retrieve the profile of the logged in user.
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return BadRequest(new
                    {
                        Error = OpenIdConnectConstants.Errors.ServerError,
                        ErrorDescription = "An internal error has occurred - User Related"
                    });
                }

                // Create a new authentication ticket.
                var ticket = await CreateTicketAsync(request, user);

                // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
                return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
            }

            //likely won't be used for ios
            [HttpGet("/connect/authorize")]
            public async Task<IActionResult> Authorize(OpenIdConnectRequest request)
            {
                if (!User.Identity.IsAuthenticated)
                {
                    // If the client application request promptless authentication,
                    // return an error indicating that the user is not logged in.
                    if (request.HasPrompt(OpenIdConnectConstants.Prompts.None))
                    {
                        var properties = new AuthenticationProperties(new Dictionary<string, string>
                        {
                            [OpenIdConnectConstants.Properties.Error] = OpenIdConnectConstants.Errors.LoginRequired,
                            [OpenIdConnectConstants.Properties.ErrorDescription] = "The user is not logged in."
                        });

                        // Ask OpenIddict to return a login_required error to the client application.
                        return Forbid(properties, OpenIdConnectServerDefaults.AuthenticationScheme);
                    }

                    return Challenge();
                }

                // Retrieve the profile of the logged in user.
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return BadRequest(new
                    {
                        Error = OpenIdConnectConstants.Errors.ServerError,
                        ErrorDescription = "An internal error has occurred"
                    });
                }

                // Create a new authentication ticket.
                var ticket = await CreateTicketAsync(request, user);

                // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
                return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
            }

        [Authorize(AuthenticationSchemes = OAuthValidationDefaults.AuthenticationScheme)]
        [HttpPost("/connect/logout")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            
            // Ask ASP.NET Core Identity to delete the local and external cookies created
            // when the user agent is redirected from the external identity provider
            // after a successful authentication flow (e.g Google or Facebook).
            await _signInManager.SignOutAsync();

            // Returning a SignOutResult will ask OpenIddict to redirect the user agent
            // to the post_logout_redirect_uri specified by the client application.
            return SignOut(OAuthValidationDefaults.AuthenticationScheme);
        }

        [HttpGet("/api/logout/{id}")]
        [Produces("application/json")]
        [Authorize(AuthenticationSchemes = OAuthValidationDefaults.AuthenticationScheme)]
        public async Task<IActionResult> LogoutApi([FromRoute] string id)
        {
            CancellationToken ct = new CancellationToken();
            ImmutableArray<OpenIddictToken> tokenList = await _tokenManager.FindBySubjectAsync(id, ct);
            var isAuthorizationsDeleted = true;
            foreach (var item in tokenList)
            {
                //var authorization = await _authorizationManager.Find
                CancellationToken ct2 = new CancellationToken();
                var authorizationId = await _tokenManager.GetAuthorizationIdAsync(item, ct2);
                var authorization = await _authorizationManager.FindByIdAsync(authorizationId, ct2);
                var deleteAuthorizationTask = _authorizationManager.DeleteAsync(authorization, ct2);
                await deleteAuthorizationTask;
                if(!deleteAuthorizationTask.IsCompletedSuccessfully){
                    isAuthorizationsDeleted = false;
                    break;
                }
            }
            if(!isAuthorizationsDeleted){
                return BadRequest( new JObject { 
                    {"success", false},
                    {"message", "Couldn't Delete Authorizations"}
                 } );
            }
            return Ok(new JObject { 
                    {"success", true},
                    {"message", "No more authorizations for this user remain. Logout Successful"}
                 } );
        }



        [HttpGet("/api/userinfo")]
        //[Authorize(ActiveAuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Userinfo()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.InvalidGrant,
                    ErrorDescription = "The user profile is no longer available."
                });
            }

            var claims = new JObject
            {
                // Note: the "sub" claim is a mandatory claim and must be included in the JSON response.
                [OpenIdConnectConstants.Claims.Subject] = user.Id,
                //[OpenIdConnectConstants.Claims.Nickname] = user.Nickname
            };

            if (User.HasClaim(OpenIdConnectConstants.Claims.Scope, OpenIdConnectConstants.Scopes.Email))
            {
                claims[OpenIdConnectConstants.Claims.Email] = user.Email;
                //claims[OpenIdConnectConstants.Claims.EmailVerified] = user.EmailConfirmed;
            }

            if (User.HasClaim(OpenIdConnectConstants.Claims.Scope, OpenIdConnectConstants.Scopes.Phone))
            {
                claims[OpenIdConnectConstants.Claims.PhoneNumber] = user.PhoneNumber;
                claims[OpenIdConnectConstants.Claims.PhoneNumberVerified] = user.PhoneNumberConfirmed;
            }

            if (User.HasClaim(OpenIdConnectConstants.Claims.Scope, OpenIddictConstants.Scopes.Roles))
            {
                //laims["roles"] = JArray.FromObject(user.ROles);
            }

            // Note: the complete list of standard claims supported by the OpenID Connect specification
            // can be found here: http://openid.net/specs/openid-connect-core-1_0.html#StandardClaims

            return Json(claims);
        }

        private async Task<AuthenticationTicket> CreateTicketAsync(OpenIdConnectRequest request, ApplicationUser user, AuthenticationProperties properties = null)
        {
            // Create a new ClaimsPrincipal containing the claims that will be used to create an id_token, a token or a code.
            var principal = await _signInManager.CreateUserPrincipalAsync(user);

            // Create a new authentication ticket holding the user identity.
            var ticket = new AuthenticationTicket(principal, properties, OpenIdConnectServerDefaults.AuthenticationScheme);

            if (!request.IsRefreshTokenGrantType())
            {
                // Set the list of scopes granted to the client application.
                // Note: the offline_access scope must be granted to allow OpenIddict to return a refresh token.
                ticket.SetScopes(new[]
                {
                    OpenIdConnectConstants.Scopes.OpenId,
                    OpenIdConnectConstants.Scopes.Email,
                    OpenIdConnectConstants.Scopes.Profile,
                    OpenIdConnectConstants.Scopes.OfflineAccess,
                    OpenIddictConstants.Scopes.Roles
                }.Intersect(request.GetScopes()));
                
            }
            ticket.SetResources("RouteManagerAPI");

            // Note: by default, claims are NOT automatically included in the access and identity tokens.
            // To allow OpenIddict to serialize them, you must attach them to a destination, that specifies
            // whether they should be included in access tokens, in identity tokens or in both.

            foreach (var claim in ticket.Principal.Claims)
            {
                // Never include the security stamp in the access and identity tokens, as it's a secret value.
                if (claim.Type == "AspNet.Identity.SecurityStamp")
                {
                    continue;
                }
                var destinations = new List<string>
                {
                    OpenIdConnectConstants.Destinations.AccessToken
                };
                // Only add the iterated claim to the id_token if the corresponding scope was granted to the client application.
                // The other claims will only be added to the access_token, which is encrypted when using the default format.
                if ((claim.Type == OpenIdConnectConstants.Claims.Name && ticket.HasScope(OpenIdConnectConstants.Scopes.Profile)) ||
                    (claim.Type == OpenIdConnectConstants.Claims.Email && ticket.HasScope(OpenIdConnectConstants.Scopes.Email)) ||
                    (claim.Type == OpenIdConnectConstants.Claims.Role && ticket.HasScope(OpenIddictConstants.Claims.Roles)))
                {
                    destinations.Add(OpenIdConnectConstants.Destinations.IdentityToken);
                }
                claim.SetDestinations(destinations);
            }
            return ticket;
        }
        private void AddErrors(IdentityResult result)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            /* [Authorize]
            //[ValidateAntiForgeryToken]
            //[FormValueRequired("Deny")]
            [HttpPost("~/Connect/Authorize")]
            public IActionResult Deny()
            {
                // Notify OpenIddict that the authorization grant has been denied by the resource owner
                // to redirect the user agent to the client application using the appropriate response_mode.
                return Forbid(OpenIdConnectServerDefaults.AuthenticationScheme);
            }
    */

            /* [HttpGet]
            public IActionResult Logout(OpenIdConnectRequest request)
            {
                // Flow the request_id to allow OpenIddict to restore
                // the original logout request from the distributed cache.
                return View(new LogoutViewModel
                {
                    RequestId = request.RequestId,
                });
            } */
    }
}
