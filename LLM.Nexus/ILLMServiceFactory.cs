using System.Collections.Generic;

namespace LLM.Nexus
{
    /// <summary>
    /// Factory for creating LLM service instances based on configuration.
    /// Supports multiple named provider configurations in the same application.
    /// </summary>
    public interface ILLMServiceFactory
    {
        /// <summary>
        /// Creates an LLM service instance for the default provider.
        /// Uses the configured DefaultProvider or the first configured provider if not specified.
        /// </summary>
        /// <returns>An instance of <see cref="ILLMService"/> for the default provider.</returns>
        ILLMService CreateService();

        /// <summary>
        /// Creates an LLM service instance for a specific named provider.
        /// </summary>
        /// <param name="providerName">The name of the provider configuration to use.</param>
        /// <returns>An instance of <see cref="ILLMService"/> for the specified provider.</returns>
        /// <exception cref="System.ArgumentException">Thrown when the provider name is null, empty, or not found in configuration.</exception>
        ILLMService CreateService(string providerName);

        /// <summary>
        /// Gets all configured provider names.
        /// </summary>
        /// <returns>A collection of configured provider names.</returns>
        IEnumerable<string> GetConfiguredProviders();

        /// <summary>
        /// Gets the name of the default provider.
        /// </summary>
        /// <returns>The name of the default provider, or the first configured provider if no default is set.</returns>
        string GetDefaultProviderName();
    }
}
