using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using P1F_IQC.Function;
using P1F_IQC.Service;
using P1F_IQC.Models;
using System.Data.SqlClient;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace P1F_IQC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;
        public AuthController(IHttpClientFactory httpClientFactory, ITokenService tokenService, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _tokenService = tokenService;
            _configuration = configuration;
        }
        [HttpGet("Index")]
        public async Task<IActionResult> Index(string originalPath = "/")
        {
            string pathBase = HttpContext.Request.PathBase;
            if (originalPath == "/")
            {
                if (!string.IsNullOrWhiteSpace(pathBase))
                {
                    originalPath = pathBase;
                }
            }
            var db = new DatabaseAccessLayer();
            var sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var first_name = User.FindFirst("firstName")?.Value;
            var last_name = User.FindFirst("lastName")?.Value;
            var full_name = first_name + " " + last_name;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var dept_id = User.FindFirst("dept_id")?.Value;
            var manager_name = User.FindFirst("manager_sesa_id")?.Value;
            //string updMsg = db.UpdateUser(sesa_id, full_name, email, manager_sesa_id, manager_name);
            List<UserDetailModel> userDetail = db.GetUserDetail(sesa_id);

            var claimsIdentity = (ClaimsIdentity)User.Identity;

            var existName = claimsIdentity?.FindFirst("p1f_iqc_name");
            if (existName != null)
            {
                claimsIdentity.RemoveClaim(existName);
            }
            var existLevel = claimsIdentity?.FindFirst("p1f_iqc_level");
            if (existLevel != null)
            {
                claimsIdentity.RemoveClaim(existLevel);
            }
            var existRole = claimsIdentity?.FindFirst("p1f_iqc_role");
            if (existRole != null)
            {
                claimsIdentity.RemoveClaim(existRole);
            }
            var existApps = claimsIdentity?.FindFirst("p1f_iqc_apps");
            if (existApps != null)
            {
                claimsIdentity.RemoveClaim(existApps);
            }

            claimsIdentity.AddClaim(new Claim("p1f_iqc_name", full_name));
            //string userRole = "TEST";

            // Check if role retrieval was successful
            if (userDetail.Any())
            {
                var user = userDetail.First();
                if (!string.IsNullOrEmpty(user.level))
                {
                    // Create a new claim for the user role
                    claimsIdentity.AddClaim(new Claim("p1f_iqc_level", user.level));
                    claimsIdentity.AddClaim(new Claim("p1f_iqc_role", user.role));
                    claimsIdentity.AddClaim(new Claim("p1f_iqc_apps", user.apps_id));
                }
                else
                {
                    claimsIdentity.AddClaim(new Claim("p1f_iqc_level", "no_access"));
                }
            }
            else
            {
                claimsIdentity.AddClaim(new Claim("p1f_iqc_level", "no_access"));
            }

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
            return Redirect(originalPath);
        }
        //[HttpGet("Login")]
        //public IActionResult Login()
        //{
        //    return Challenge(new AuthenticationProperties { RedirectUri = Url.Action("GetUserProfile") });
        //}
        [HttpGet("test-refresh")]
        public async Task<IActionResult> TestRefresh(string refreshToken)
        {
            var result = await _tokenService.RefreshAccessToken(refreshToken, HttpContext);
            return Ok(result);
        }
        [HttpGet("RefreshToken")]
        public async Task<IActionResult> RefreshToken()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!result.Succeeded)
            {
                return BadRequest("Authentication failed.");
            }

            var refreshToken = result.Properties.GetTokenValue("refresh_token");

            if (string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest("No refresh token found.");
            }

            var client = _httpClientFactory.CreateClient();

            var tokenRequest = new Dictionary<string, string>
            {
                { "grant_type", "refresh_token" },
                { "refresh_token", refreshToken },
                { "client_id", _configuration["Auth:ClientId"] },
                { "client_secret", _configuration["Auth:ClientSecret"] }
            };

            var tokenResponse = await client.PostAsync(_configuration["Auth:TokenEndpoint"], new FormUrlEncodedContent(tokenRequest));

            if (!tokenResponse.IsSuccessStatusCode)
            {
                return BadRequest("Token refresh failed.");
            }

            var newTokenResponse = await tokenResponse.Content.ReadAsStringAsync();
            var newTokens = JsonSerializer.Deserialize<RefreshTokenResponse>(newTokenResponse);

            // Save new tokens in the authentication properties (optional)
            result.Properties.UpdateTokenValue("access_token", newTokens.access_token);
            if (!string.IsNullOrEmpty(newTokens.refresh_token))
            {
                result.Properties.UpdateTokenValue("refresh_token", newTokens.refresh_token);
            }

            // Sign in the user again with the updated tokens
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, result.Principal, result.Properties);

            return RedirectToAction("TokenInfo"); ;
        }

        //[Authorize]
        //[HttpGet("TokenInfo")]
        //public async Task<IActionResult> TokenInfo()
        //{
        //    var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        //    //var userName = User.Identity.Name;
        //    if (!result.Succeeded)
        //    {
        //        return BadRequest("Authentication failed.");
        //    }
        //    var accessToken = result.Properties.GetTokenValue("access_token");
        //    var refreshToken = result.Properties.GetTokenValue("refresh_token");

        //    return Ok(new { AccessToken = accessToken, RefreshToken = refreshToken });
        //}
        //[Authorize]
        [HttpGet("CheckClaim")]
        public async Task<IActionResult> CheckClaim()
        {
            string sesa_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            string name = User.FindFirst("p1f_iqc_name")?.Value;
            return Ok(sesa_id + " - " + name);
        }
        [Authorize]
        [HttpGet("GetUserProfile")]
        public async Task<IActionResult> GetUserProfile()
        {
            //var sub = User.FindFirst("preferred_username")?.Value;
            //var name = User.FindFirst("company")?.Value;
            //return Ok(new { sesa_id = sub, name = name });
            //var result = await HttpContext.AuthenticateAsync();
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var accessToken = result.Properties.GetTokenValue("access_token");
            ////var accessToken = await HttpContext.GetTokenAsync("access_token");
            if (!result.Succeeded)
            {
                return BadRequest("Authentication failed.");
            }
            string userInfoEndpoint = _configuration["Auth:UserInfoEndpoint"];
            var client = _httpClientFactory.CreateClient();

            // Set the authorization header with the access token
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //client.DefaultRequestHeaders.Host = "ping-sso-uat.schneider-electric.com";

            // Make the request to the user info endpoint
            HttpResponseMessage response = await client.GetAsync(userInfoEndpoint);

            if (!response.IsSuccessStatusCode)
            {
                return BadRequest("Failed to retrieve user profile.");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var userProfile = JsonSerializer.Deserialize<dynamic>(jsonResponse);

            //{"sub":"SESA546923","lastName":"WIRYADINATA","manager":"SESA768540","employeeID":"SESA546923","preferred_username":"SESA546923","managerName":"RAMADHAN, Aditya Kurnia",
            //"title":"RPA & SOFTWARE DEVELOPER","employeeNumber":"10815193","division":"Global Supply Chain","firstName":"ADIEL","company":"PT SCHNEIDER ELECTRIC MANUFACTURING BATAM",
            //"state":"Indonesia","email":"Adiel.WIRYADINATA@se.com"}
            //var userId = userProfile?.sub;
            var profile = userProfile;

            return Ok(new { Profile = profile });
        }

        [HttpGet("Logout")]
        public IActionResult Logout()
        {
            return SignOut(new AuthenticationProperties { RedirectUri = Url.Action("Index", "Home") }, CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }

    public class UserProfile
    {
        public string sub { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string manager { get; set; }
        public string managerName { get; set; }
    }
    public class RefreshTokenResponse
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
    }
}
