using LLM.Nexus.Providers.Anthropic;
using LLM.Nexus.Providers.OpenAI;
using LLM.Nexus.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace LLM.Nexus
{
    /// <summary>
    /// Provides extension methods for registering LLM services with dependency injection.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Adds LLM services to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddLLMServices(this IServiceCollection services)
        {
            services.AddOptions<LLMSettings>()
                        .BindConfiguration(LLMSettings.Section)
                        .ValidateDataAnnotations()
                        .ValidateOnStart();

            services.AddSingleton<IAnthropicService, AnthropicService>();
            services.AddSingleton<IOpenAIService, OpenAIService>();
            services.AddSingleton<ILLMServiceFactory, LLMServiceFactory>();

            return services;
        }
    }
}
