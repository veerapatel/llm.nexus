using System.ComponentModel.DataAnnotations;

namespace LLM.Nexus.Settings
{
    /// <summary>
    /// Configuration settings for LLM providers.
    /// </summary>
    public class LLMSettings
    {
        /// <summary>
        /// The configuration section name for LLM settings.
        /// </summary>
        public const string Section = "LLMSettings";

        /// <summary>
        /// Gets or sets the LLM provider to use (OpenAI, Anthropic, etc.).
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
