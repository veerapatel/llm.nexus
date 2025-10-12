namespace LLM.Nexus
{
    /// <summary>
    /// Factory for creating LLM service instances based on configuration.
    /// </summary>
    public interface ILLMServiceFactory
    {
        /// <summary>
        /// Creates an LLM service instance based on the configured provider.
        /// </summary>
        /// <returns>An instance of <see cref="ILLMService"/> for the configured provider.</returns>
        ILLMService CreateService();
    }
}
