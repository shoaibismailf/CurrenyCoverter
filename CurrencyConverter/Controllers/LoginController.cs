using Currency.Application.Helpers;
using Currency.Application.Models.Request;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Currency.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [AllowAnonymous]
        [HttpPost("GetToken")]
        public async Task<IActionResult> GetToken([FromBody] TokenRequest requestModel)
        {
            if (requestModel == null ||
            string.IsNullOrEmpty(requestModel.Username) || string.IsNullOrEmpty(requestModel.Password))
            {
                return BadRequest("Username, Password are required.");
            }

            using var httpClient = new HttpClient();

            var keyValues = new List<KeyValuePair<string, string>>
            {
                new("password", requestModel.Password),
                new("username", requestModel.Username),
                new("client_id", _configuration["IdentityServerConfig:ClientId"]!),
                new("client_secret", _configuration["IdentityServerConfig:ClientSecret"]!),
                new("grant_type", "password"),
                new("scope", _configuration["IdentityServerConfig:currency_api"]!)
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_configuration["Token:Authority"]}/connect/token")
            {
                Content = new FormUrlEncodedContent(keyValues)
            };

            var response = await httpClient.SendAsync(httpRequest);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, error);
            }

            var tokenResponse = await response.Content.ReadAsStringAsync();
            return Ok(tokenResponse);
        }
    }
}
