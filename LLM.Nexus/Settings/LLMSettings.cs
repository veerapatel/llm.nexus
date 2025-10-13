using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LLM.Nexus.Settings
{
    /// <summary>
    /// Configuration settings for LLM providers.
    /// Supports multiple named provider configurations in the same application.
    /// </summary>
    public class LLMSettings
    {
        /// <summary>
        /// The configuration section name for LLM settings.
        /// </summary>
        public const string Section = "LLMSettings";

        /// <summary>
        /// Gets or sets multiple named provider configurations.
        /// This enables multi-provider support in the same application.
        /// </summary>
        [Required]
        [MinLength(1, ErrorMessage = "At least one provider must be configured.")]
        public Dictionary<string, ProviderConfiguration> Providers { get; set; } = new Dictionary<string, ProviderConfiguration>();

        /// <summary>
        /// Gets or sets the name of the default provider to use when not specified.
        /// If not set, the first configured provider will be used as default.
        /// </summary>
        public string DefaultProvider { get; set; } = string.Empty;
    }
}
