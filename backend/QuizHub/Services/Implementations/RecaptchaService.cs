using System.Text.Json;
using QuizHub.Services.Interfaces;

namespace QuizHub.Services.Implementations
{
    public class RecaptchaService : IRecaptchaService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RecaptchaService> _logger;

        public RecaptchaService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<RecaptchaService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> VerifyTokenAsync(
            string? token,
            string? remoteIp,
            string? expectedAction = null,
            CancellationToken cancellationToken = default)
        {
            var enabled = _configuration.GetValue<bool>("Recaptcha:Enabled");
            if (!enabled)
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            var secretKey = _configuration["Recaptcha:SecretKey"];
            if (string.IsNullOrWhiteSpace(secretKey))
            {
                _logger.LogWarning("Recaptcha is enabled but Recaptcha:SecretKey is missing.");
                return false;
            }

            var verifyUrl = _configuration["Recaptcha:VerifyUrl"]
                ?? "https://www.google.com/recaptcha/api/siteverify";
            var minimumScore = _configuration.GetValue<double?>("Recaptcha:MinimumScore") ?? 0.5;
            var recaptchaVersion = (_configuration["Recaptcha:Version"] ?? "v2").ToLowerInvariant();
            var useV3Checks = recaptchaVersion == "v3";

            try
            {
                var formData = new Dictionary<string, string>
                {
                    ["secret"] = secretKey,
                    ["response"] = token
                };

                if (!string.IsNullOrWhiteSpace(remoteIp))
                {
                    formData["remoteip"] = remoteIp;
                }

                using var request = new HttpRequestMessage(HttpMethod.Post, verifyUrl)
                {
                    Content = new FormUrlEncodedContent(formData)
                };

                var client = _httpClientFactory.CreateClient();
                using var response = await client.SendAsync(request, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("reCAPTCHA verify endpoint returned status code {StatusCode}", (int)response.StatusCode);
                    return false;
                }

                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonSerializer.Deserialize<RecaptchaVerifyResponse>(body, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result is null || !result.Success)
                {
                    _logger.LogWarning("reCAPTCHA verification failed. Errors: {Errors}",
                        result?.ErrorCodes is null ? "none" : string.Join(", ", result.ErrorCodes));
                    return false;
                }

                if (useV3Checks
                    && !string.IsNullOrWhiteSpace(expectedAction)
                    && !string.IsNullOrWhiteSpace(result.Action)
                    && !string.Equals(result.Action, expectedAction, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("reCAPTCHA action mismatch. Expected {Expected}, got {Actual}", expectedAction, result.Action);
                    return false;
                }

                if (useV3Checks && result.Score.HasValue && result.Score.Value < minimumScore)
                {
                    _logger.LogWarning("reCAPTCHA score too low. Score: {Score}, minimum: {Minimum}", result.Score.Value, minimumScore);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while verifying reCAPTCHA token");
                return false;
            }
        }

        private sealed class RecaptchaVerifyResponse
        {
            public bool Success { get; set; }
            public double? Score { get; set; }
            public string? Action { get; set; }
            public string[]? ErrorCodes { get; set; }
        }
    }
}