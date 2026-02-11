using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Elisoft.Teams.Services
{
    public class TeamsNotificator : ITeamsNotificator
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TeamsNotificator> _logger;

        public TeamsNotificator(HttpClient httpClient, ILogger<TeamsNotificator> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<bool> SendMessageAsync(string webhookUrl, string messageText)
        {
            if (string.IsNullOrWhiteSpace(webhookUrl))
            {
                _logger.LogError("WebhookUrl is required.");
                throw new ArgumentException(nameof(webhookUrl));
            }

            if (!Uri.TryCreate(webhookUrl, UriKind.Absolute, out _))
            {
                _logger.LogError("Incorrect format WebhookUrl.");
                throw new ArgumentException(nameof(webhookUrl));
            }

            if (string.IsNullOrWhiteSpace(messageText))
            {
                _logger.LogError("The message content is required.");
                throw new ArgumentException(nameof(messageText));
            }

            var teamsPayloadObject = new
            {
                text = messageText
            };

            var jsonPayload = JsonSerializer.Serialize(teamsPayloadObject);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(webhookUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error Teams Api ({StatusCode}): {Error}", response.StatusCode, error);
                    return false;
                }

                _logger.LogInformation("Notification sent successfully.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception when communicating with Teams.");
                return false;
            }
        }
    }
}
