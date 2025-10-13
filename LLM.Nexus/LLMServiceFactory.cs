using System;
using System.Collections.Generic;
using System.Linq;
using LLM.Nexus.Providers.Anthropic;
using LLM.Nexus.Providers.Google;
using LLM.Nexus.Providers.OpenAI;
using LLM.Nexus.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LLM.Nexus
{
    /// <summary>
    /// Factory implementation for creating LLM service instances based on configuration.
    /// Supports multiple named provider configurations in the same application.
    /// </summary>
    public class LLMServiceFactory : ILLMServiceFactory
    {
        private readonly LLMSettings _settings;
        private readonly ILoggerFactory _loggerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="LLMServiceFactory"/> class.
        /// </summary>
        /// <param name="loggerFactory">The logger factory for creating provider loggers.</param>
        /// <param name="settings">The LLM configuration settings.</param>
        public LLMServiceFactory(ILoggerFactory loggerFactory, IOptions<LLMSettings> settings)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));

            if (_settings.Providers == null || _settings.Providers.Count == 0)
            {
                throw new InvalidOperationException("At least one provider must be configured in LLMSettings.Providers.");
            }

            // Validate DefaultProvider if specified
            if (!string.IsNullOrWhiteSpace(_settings.DefaultProvider) && !_settings.Providers.ContainsKey(_settings.DefaultProvider))
            {
                throw new InvalidOperationException($"The configured DefaultProvider '{_settings.DefaultProvider}' was not found in the Providers dictionary.");
            }
        }

        /// <inheritdoc/>
        public ILLMService CreateService()
        {
            string providerName = GetDefaultProviderName();
            return CreateService(providerName);
        }

        /// <inheritdoc/>
        public ILLMService CreateService(string providerName)
        {
            if (string.IsNullOrWhiteSpace(providerName))
            {
                throw new ArgumentException("Provider name cannot be null or empty.", nameof(providerName));
            }

            if (!_settings.Providers.TryGetValue(providerName, out ProviderConfiguration? config))
            {
                throw new ArgumentException($"Provider '{providerName}' not found in configuration. Available providers: {string.Join(", ", _settings.Providers.Keys)}");
            }

            return CreateServiceForProvider(config);
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetConfiguredProviders()
        {
            return _settings.Providers.Keys.ToList();
        }

        /// <inheritdoc/>
        public string GetDefaultProviderName()
        {
            return !string.IsNullOrWhiteSpace(_settings.DefaultProvider)
                ? _settings.DefaultProvider
                : _settings.Providers.Keys.First();
        }

        /// <summary>
        /// Creates a service instance for a specific provider configuration.
        /// </summary>
        private LLMServiceAdapter CreateServiceForProvider(ProviderConfiguration config)
        {
#pragma warning disable CA2000 // Dispose objects before losing scope - service ownership transferred to adapter
            switch (config.Provider)
            {
                case LLMProvider.OpenAI:
                    var openAILogger = _loggerFactory.CreateLogger<OpenAIService>();
                    var openAIService = new OpenAIService(openAILogger, config);
                    return new LLMServiceAdapter(openAIService);

                case LLMProvider.Anthropic:
                    var anthropicLogger = _loggerFactory.CreateLogger<AnthropicService>();
                    var anthropicService = new AnthropicService(anthropicLogger, config);
                    return new LLMServiceAdapter(anthropicService);

                case LLMProvider.Google:
                    var googleLogger = _loggerFactory.CreateLogger<GoogleService>();
                    var googleService = new GoogleService(googleLogger, config);
                    return new LLMServiceAdapter(googleService);

                default:
                    throw new ArgumentException($"Unsupported LLM provider: {config.Provider}");
            }
#pragma warning restore CA2000
        }
    }
}
