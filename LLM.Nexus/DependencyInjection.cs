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
        /// Registers the LLM service factory that supports multiple provider configurations.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddLLMServices(this IServiceCollection services)
        {
            services.AddOptions<LLMSettings>()
                        .BindConfiguration(LLMSettings.Section)
                        .ValidateDataAnnotations()
                        .ValidateOnStart();

            services.AddSingleton<ILLMServiceFactory, LLMServiceFactory>();

            return services;
        }
    }
}
