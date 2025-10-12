using System;
using LLM.Nexus.Providers.Anthropic;
using LLM.Nexus.Providers.OpenAI;
using LLM.Nexus.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LLM.Nexus
{
    /// <summary>
    /// Factory implementation for creating LLM service instances based on configuration.
    /// </summary>
    public class LLMServiceFactory : ILLMServiceFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly LLMSettings _settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="LLMServiceFactory"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider for resolving dependencies.</param>
        /// <param name="settings">The LLM configuration settings.</param>
        public LLMServiceFactory(IServiceProvider serviceProvider, IOptions<LLMSettings> settings)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        }

        /// <inheritdoc/>
        public ILLMService CreateService()
        {
            switch (_settings.Provider)
            {
                case LLMProvider.OpenAI:
                    return new LLMServiceAdapter(_serviceProvider.GetRequiredService<IOpenAIService>());
                case LLMProvider.Anthropic:
                    return new LLMServiceAdapter(_serviceProvider.GetRequiredService<IAnthropicService>());
                default:
                    throw new ArgumentException($"Unsupported LLM provider: {_settings.Provider}");
            }
        }
    }
}
