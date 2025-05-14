namespace SocialWelfarre.Services
{
    public class SmsService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl = "https://sms.iprogtech.com/api/v1/sms_messages";
        private readonly string _apiToken = "397245949647432934da9469fd106e005cc49b9a"; // Using the provided API token

        public SmsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task SendSmsAsync(string phoneNumber, string message, int smsProvider = 0)
        {
            var url = $"{_apiUrl}?api_token={_apiToken}" +
                      $"&message={Uri.EscapeDataString(message)}" +
                      $"&phone_number={phoneNumber}" +
                      $"&sms_provider={smsProvider}";

            // Send the request
            var response = await _httpClient.PostAsync(url, null); // No body content, just query string

            if (!response.IsSuccessStatusCode)
            {
                // Log the error and response details for debugging
                var errorDetails = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to send SMS. Status Code: {response.StatusCode}, Response: {errorDetails}");
            }

            // Optionally, log success
            Console.WriteLine("SMS sent successfully.");
        }
    }
}
