using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using LLM.Nexus.Models;
using LLM.Nexus.Settings;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;

namespace LLM.Nexus.Providers.OpenAI
{
    internal class OpenAIService : IOpenAIService
    {
        private readonly ILogger<OpenAIService> _logger;
        private readonly ProviderConfiguration _config;
        private readonly ChatClient _client;

        public OpenAIService(ILogger<OpenAIService> logger, ProviderConfiguration config)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));

            // Initialize client once and reuse it
            _client = new ChatClient(_config.Model, _config.ApiKey);
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
                _logger.LogInformation("Generating OpenAI response for prompt with {PromptLength} characters", request.Prompt.Length);

                var chatOptions = new ChatCompletionOptions
                {
                    MaxOutputTokenCount = request.MaxTokens ?? _config.MaxTokens
                };

                if (request.Temperature.HasValue)
                {
                    chatOptions.Temperature = (float)request.Temperature.Value;
                }

                var messages = new[] { new UserChatMessage(request.Prompt) };
                var completion = await _client.CompleteChatAsync(messages, chatOptions, cancellationToken).ConfigureAwait(false);

                var response = new LLMResponse
                {
                    Content = completion.Value.Content[0].Text,
                    Id = completion.Value.Id,
                    Model = completion.Value.Model,
                    Provider = "OpenAI",
                    Timestamp = DateTimeOffset.UtcNow,
                    FinishReason = completion.Value.FinishReason.ToString(),
                    Usage = new UsageInfo
                    {
                        PromptTokens = completion.Value.Usage.InputTokenCount,
                        CompletionTokens = completion.Value.Usage.OutputTokenCount,
                        TotalTokens = completion.Value.Usage.TotalTokenCount
                    }
                };

                _logger.LogInformation("Successfully generated OpenAI response. Tokens used: {TotalTokens}", response.Usage.TotalTokens);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating OpenAI response.");
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
