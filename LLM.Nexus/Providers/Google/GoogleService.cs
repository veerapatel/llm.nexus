using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.AIPlatform.V1;
using LLM.Nexus.Models;
using LLM.Nexus.Settings;
using Microsoft.Extensions.Logging;

namespace LLM.Nexus.Providers.Google
{
    internal class GoogleService : IGoogleService
    {
        private readonly ILogger<GoogleService> _logger;
        private readonly ProviderConfiguration _config;
        private readonly PredictionServiceClient _client;

        public GoogleService(ILogger<GoogleService> logger, ProviderConfiguration config)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));

            // Initialize client once and reuse it
            _client = PredictionServiceClient.Create();
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

                // Build the request for Gemini
                var content = new Content();
                content.Parts.Add(new Part { Text = request.Prompt });

                var generateContentRequest = new GenerateContentRequest
                {
                    Model = _config.Model,
                    Contents = { content }
                };

                // Add generation config if parameters are specified
                if (request.MaxTokens.HasValue || request.Temperature.HasValue)
                {
                    var generationConfig = new GenerationConfig();

                    if (request.MaxTokens.HasValue)
                    {
                        generationConfig.MaxOutputTokens = request.MaxTokens.Value;
                    }

                    if (request.Temperature.HasValue)
                    {
                        generationConfig.Temperature = (float)request.Temperature.Value;
                    }

                    generateContentRequest.GenerationConfig = generationConfig;
                }

                // Add system instruction if specified
                if (!string.IsNullOrWhiteSpace(request.SystemMessage))
                {
                    generateContentRequest.SystemInstruction = new Content
                    {
                        Parts = { new Part { Text = request.SystemMessage } }
                    };
                }

                var apiResponse = await _client.GenerateContentAsync(generateContentRequest, cancellationToken).ConfigureAwait(false);

                var responseText = apiResponse.Candidates.Count > 0 && apiResponse.Candidates[0].Content.Parts.Count > 0
                    ? apiResponse.Candidates[0].Content.Parts[0].Text
                    : string.Empty;

                var response = new LLMResponse
                {
                    Content = responseText,
                    Id = Guid.NewGuid().ToString(), // Google doesn't provide a unique ID in the response
                    Model = _config.Model,
                    Provider = "Google",
                    Timestamp = DateTimeOffset.UtcNow,
                    FinishReason = apiResponse.Candidates.Count > 0
                        ? apiResponse.Candidates[0].FinishReason.ToString()
                        : string.Empty,
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
