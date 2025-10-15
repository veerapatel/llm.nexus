using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GenerativeAI;
using GenerativeAI.Types;
using LLM.Nexus.Models;
using LLM.Nexus.Settings;
using Microsoft.Extensions.Logging;

namespace LLM.Nexus.Providers.Google
{
    internal class GoogleService : IGoogleService
    {
        private readonly ILogger<GoogleService> _logger;
        private readonly ProviderConfiguration _config;
        private readonly GoogleAi _client;

        public GoogleService(ILogger<GoogleService> logger, ProviderConfiguration config)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));

            if (string.IsNullOrWhiteSpace(_config.ApiKey))
            {
                throw new ArgumentException("Google API key is required", nameof(config));
            }

            // Initialize client with API key
            _client = new GoogleAi(_config.ApiKey);
        }

        public async Task<LLMResponse> GenerateResponseAsync(LLMRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            // Validate the request
            var validationContext = new ValidationContext(request);
            Validator.ValidateObject(request, validationContext, validateAllProperties: true);

            try
            {
                _logger.LogInformation("Generating Google (Gemini) response for prompt with {PromptLength} characters", request.Prompt.Length);

                // Ensure model name has correct format
                var modelName = _config.Model;
                if (!modelName.StartsWith("models/", StringComparison.Ordinal))
                {
                    modelName = $"models/{modelName}";
                }

                // Build generation config
                var generationConfig = new GenerationConfig();

                if (request.Temperature.HasValue)
                {
                    generationConfig.Temperature = (float)request.Temperature.Value;
                }

                if (request.MaxTokens.HasValue)
                {
                    generationConfig.MaxOutputTokens = request.MaxTokens.Value;
                }

                // Create model with configuration
                var model = _client.CreateGenerativeModel(modelName, generationConfig);

                // Set system instruction if specified
                if (!string.IsNullOrWhiteSpace(request.SystemMessage))
                {
                    model.SystemInstruction = request.SystemMessage;
                }

                // Generate content
                var apiResponse = await model.GenerateContentAsync(request.Prompt, cancellationToken).ConfigureAwait(false);

                var responseText = apiResponse.Text() ?? string.Empty;

                var response = new LLMResponse
                {
                    Content = responseText,
                    Id = Guid.NewGuid().ToString(), // Google doesn't provide a unique ID in the response
                    Model = _config.Model,
                    Provider = "Google",
                    Timestamp = DateTimeOffset.UtcNow,
                    FinishReason = apiResponse.Candidates?.FirstOrDefault()?.FinishReason.ToString() ?? string.Empty,
                    Usage = new UsageInfo
                    {
                        PromptTokens = apiResponse.UsageMetadata?.PromptTokenCount ?? 0,
                        CompletionTokens = apiResponse.UsageMetadata?.CandidatesTokenCount ?? 0,
                        TotalTokens = apiResponse.UsageMetadata?.TotalTokenCount ?? 0
                    }
                };

                _logger.LogInformation("Successfully generated Google (Gemini) response. Tokens used: {TotalTokens}", response.Usage.TotalTokens);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Google (Gemini) response.");
                throw;
            }
        }

        public async Task<LLMResponse> GenerateResponseAsync(string prompt, CancellationToken cancellationToken = default)
        {
            var request = new LLMRequest
            {
                Prompt = prompt
            };

            return await GenerateResponseAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
