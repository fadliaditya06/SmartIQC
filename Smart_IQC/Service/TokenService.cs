using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using OfficeOpenXml;

namespace Smart_IQC.Service
{
    public interface ITokenService
    {
        Task<bool> RefreshAccessToken(string refreshToken, HttpContext httpContext);
    }

    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> RefreshAccessToken(string refreshToken, HttpContext httpContext)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var tokenRequest = new Dictionary<string, string>
                    {
                        { "grant_type", "refresh_token" },
                        { "refresh_token", refreshToken },
                        { "client_id", _configuration["Auth:ClientId"] },
                        { "client_secret", _configuration["Auth:ClientSecret"] },
                        { "scope", "openid profile" } 
                    };

                    var requestContent = new FormUrlEncodedContent(tokenRequest);
                    var tokenEndpoint = $"{_configuration["Auth:TokenEndpoint"]}"; 
                    var response = await httpClient.PostAsync(tokenEndpoint, requestContent);

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var tokenResponse = JsonSerializer.Deserialize<RefreshTokenResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (tokenResponse != null)
                        {
                            httpContext.Response.Cookies.Append("access_token", tokenResponse.access_token, new CookieOptions
                            {
                                HttpOnly = true,
                                Secure = true, 
                                SameSite = SameSiteMode.Strict 
                            });

                            if (!string.IsNullOrEmpty(tokenResponse.refresh_token))
                            {
                                httpContext.Response.Cookies.Append("refresh_token", tokenResponse.refresh_token, new CookieOptions
                                {
                                    HttpOnly = true,
                                    Secure = true,
                                    SameSite = SameSiteMode.Strict
                                });
                            }

                            return true; 
                        }
                    }
                }
            }
            catch (HttpRequestException httpRequestException)
            {
                Console.WriteLine($"HTTP Request error: {httpRequestException.Message}");
            }
            catch (JsonException jsonException)
            {
                Console.WriteLine($"JSON Deserialization error: {jsonException.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            return false; 
        }
    }
    public class RefreshTokenResponse
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
    }
}
