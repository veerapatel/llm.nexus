using System.ComponentModel.DataAnnotations;

namespace LLM.Nexus.Settings
{
    /// <summary>
    /// Configuration for a specific LLM provider instance.
    /// </summary>
    public class ProviderConfiguration
    {
        /// <summary>
        /// Gets or sets the LLM provider type (OpenAI, Anthropic, Google, etc.).
        /// </summary>
        [Required]
        public LLMProvider Provider { get; set; }

        /// <summary>
        /// Gets or sets the API key for authenticating with the LLM provider.
        /// </summary>
        [Required]
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the model identifier to use (e.g., "gpt-4", "claude-sonnet-4-5-20250929").
        /// </summary>
        [Required]
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the maximum number of tokens to generate in the response.
        /// Defaults to 2000 if not specified.
        /// </summary>
        public int? MaxTokens { get; set; } = 2000;

        /// <summary>
        /// Gets or sets whether to use streaming for responses.
        /// </summary>
        public bool? Stream { get; set; }
    }
}
