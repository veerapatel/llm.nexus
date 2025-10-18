using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LLM.Nexus.Models
{
    /// <summary>
    /// Represents a request to an LLM provider.
    /// </summary>
    public class LLMRequest
    {
        /// <summary>
        /// Gets or sets the prompt/user message to send to the LLM.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        [StringLength(1000000, MinimumLength = 1, ErrorMessage = "Prompt must be between 1 and 1,000,000 characters.")]
        public string Prompt { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the system message/instructions (optional).
        /// </summary>
        public string SystemMessage { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the maximum number of tokens to generate (optional, uses provider default if not specified).
        /// </summary>
        [Range(1, 1000000)]
        public int? MaxTokens { get; set; }

        /// <summary>
        /// Gets or sets the temperature for response generation (0.0 to 2.0, optional).
        /// </summary>
        [Range(0.0, 2.0)]
        public double? Temperature { get; set; }

        /// <summary>
        /// Gets or sets additional provider-specific parameters (optional).
        /// </summary>
        public Dictionary<string, object> AdditionalParameters { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the list of files/media to include with the request (optional).
        /// </summary>
        public List<FileContent> Files { get; set; } = new List<FileContent>();
    }
}
