using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Anthropic.SDK;
using Anthropic.SDK.Messaging;
using LLM.Nexus.Models;
using LLM.Nexus.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LLM.Nexus.Providers.Anthropic
{
    internal class AnthropicService : IAnthropicService, IDisposable
    {
        private readonly LLMSettings _settings;
        private readonly ILogger<AnthropicService> _logger;
        private readonly AnthropicClient _client;
        private bool _disposed;

        public AnthropicService(ILogger<AnthropicService> logger, IOptions<LLMSettings> settings)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));

            // Initialize client once and reuse it
            _client = new AnthropicClient(_settings.ApiKey);
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
                _logger.LogInformation("Generating Anthropic response for prompt with {PromptLength} characters", request.Prompt.Length);

                var messages = new List<Message> { new Message(RoleType.User, request.Prompt) };

                var parameters = new MessageParameters()
                {
                    Messages = messages,
                    Model = _settings.Model,
                    MaxTokens = request.MaxTokens ?? _settings.MaxTokens ?? 2000,
                    Stream = _settings.Stream
                };

                if (!string.IsNullOrWhiteSpace(request.SystemMessage))
                {
                    parameters.System = new List<SystemMessage> { new SystemMessage(request.SystemMessage) };
                }

                if (request.Temperature.HasValue)
                {
                    parameters.Temperature = (decimal)request.Temperature.Value;
                }

                var apiResponse = await _client.Messages.GetClaudeMessageAsync(parameters, cancellationToken).ConfigureAwait(false);

                var textContent = apiResponse.Content[0] as TextContent;
                var content = textContent?.Text ?? string.Empty;

                var response = new LLMResponse
                {
                    Content = content,
                    Id = apiResponse.Id,
                    Model = apiResponse.Model,
                    Provider = "Anthropic",
                    Timestamp = DateTimeOffset.UtcNow,
                    FinishReason = apiResponse.StopReason ?? string.Empty,
                    StopSequence = apiResponse.StopSequence?.ToString() ?? string.Empty,
                    Usage = new UsageInfo
                    {
                        PromptTokens = apiResponse.Usage.InputTokens,
                        CompletionTokens = apiResponse.Usage.OutputTokens,
                        TotalTokens = apiResponse.Usage.InputTokens + apiResponse.Usage.OutputTokens
                    }
                };

                _logger.LogInformation("Successfully generated Anthropic response. Tokens used: {TotalTokens}", response.Usage.TotalTokens);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Anthropic response.");
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _client?.Dispose();
                }

                _disposed = true;
            }
        }
    }
}
