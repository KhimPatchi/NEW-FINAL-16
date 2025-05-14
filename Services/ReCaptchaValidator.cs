// Services/ReCaptchaValidator.cs
using Microsoft.Extensions.Options;
using SocialWelfarre.Models;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace SocialWelfarre.Services
{
    public class ReCaptchaValidator
    {
        private readonly GoogleReCaptchaSettings _settings;
        private readonly IHttpClientFactory _httpClientFactory;

        public ReCaptchaValidator(IOptions<GoogleReCaptchaSettings> settings, IHttpClientFactory httpClientFactory)
        {
            _settings = settings.Value;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<bool> IsCaptchaPassedAsync(string token)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetStringAsync(
                $"https://www.google.com/recaptcha/api/siteverify?secret={_settings.SecretKey}&response={token}");

            var result = JsonSerializer.Deserialize<ReCaptchaResponse>(response);
            return result.Success && result.Score >= 0.5;
        }

        private class ReCaptchaResponse
        {
            public bool Success { get; set; }
            public float Score { get; set; }
            public string Action { get; set; }
        }
    }
}
