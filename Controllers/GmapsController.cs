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
using Microsoft.AspNetCore.Http.Extensions;

namespace api.cabcheap.com.Controllers
{
    public class GmapsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
            private readonly SignInManager<ApplicationUser> _signInManager;
            private readonly IEmailSender _emailSender;
            private readonly ILogger _logger;
            private readonly IConfiguration _config;



            private readonly ApplicationDbContext _ctx;

            private readonly string _googleApiKey;
            private const string GoogleApiTokenInfoUrl = "https://www.googleapis.com/oauth2/v3/tokeninfo?id_token={0}";
            private const string GoogleApiUserInfoUrl = "https://www.googleapis.com/plus/v1/people/{0}?userIp={1}&key={2}";

            private const string GoogleDirectionsApiUrl = "https://maps.googleapis.com/maps/api/directions/json";
            private const string GooglePlacesAutoCompleteApiUrl = "https://maps.googleapis.com/maps/api/place/autocomplete/json";

            private const string GooglePlacesDetailsApiUrl = "https://maps.googleapis.com/maps/api/place/details/json";


            

            public GmapsController(
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
                var GoogleConfig = _config.GetSection("ExternalIdentities").GetSection("Google");
                _googleApiKey = GoogleConfig["api_key"];
            }

        [HttpGet("/gmaps/places")]
        [Produces("application/json")]
        //[Authorize(AuthenticationSchemes = OAuthValidationDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetAutoComplete([FromQuery] string input)
        {
            if(input.CompareTo("")==0){
                return BadRequest( new JObject { 
                    {"success", false},
                    {"message", "input is empty, cannot look up place"}
                 } );
            }
            var response = await getPlacesHttpResponseResult(input);
            if(response != null){
                return Ok(response);
            }else {
                return BadRequest( new JObject { 
                    {"success", false},
                    {"message", "Request for place was bad"}
                 } );   
            }
        }



        public async Task<JObject> getPlacesHttpResponseResult(string input){
            var httpClient = new HttpClient();
            var qb = new QueryBuilder();
            qb.Add("input", input);
            qb.Add("key", _googleApiKey);
            
            Uri RequestUri = new Uri(GooglePlacesAutoCompleteApiUrl + qb.ToQueryString());
            // new Uri(string.Format(GoogleApiTokenInfoUrl, providerToken));
            
            
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
            httpResponseMessage = await httpClient.GetAsync(RequestUri);

            if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
            {
                //Console.WriteLine(ex.Message);
                return null;
            }

            var response = JObject.Parse(httpResponseMessage.Content.ReadAsStringAsync().Result);
            return response;
        }

    }
}
