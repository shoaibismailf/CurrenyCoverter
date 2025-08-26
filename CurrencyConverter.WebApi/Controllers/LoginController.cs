using Currency.Application.Models.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
                string.IsNullOrWhiteSpace(requestModel.Username) ||
                string.IsNullOrWhiteSpace(requestModel.Password) ||
                string.IsNullOrWhiteSpace(requestModel.ClientId) ||
                string.IsNullOrWhiteSpace(requestModel.ClientSecret))
            {
                return BadRequest("Username, Password, ClientId, and ClientSecret are required.");
            }

            using var httpClient = new HttpClient();

            var keyValues = new List<KeyValuePair<string, string>>
            {
                new("password", requestModel.Password),
                new("username", requestModel.Username),
                new("client_id", requestModel.ClientId),
                new("client_secret", requestModel.ClientSecret),
                new("grant_type", "password"),
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
