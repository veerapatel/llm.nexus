using System;

namespace LLM.Nexus.Models
{
    /// <summary>
    /// Represents the response from an LLM provider.
    /// </summary>
    public class LLMResponse
    {
        /// <summary>
        /// Gets or sets the generated text content.
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the unique identifier for this response from the provider.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the model that generated the response.
        /// </summary>
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the provider that generated the response.
        /// </summary>
        public string Provider { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the usage information (tokens consumed).
        /// </summary>
        public UsageInfo Usage { get; set; } = new UsageInfo();

        /// <summary>
        /// Gets or sets the timestamp when the response was generated.
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the finish/stop reason (completed, length, stop, etc.).
        /// </summary>
        public string FinishReason { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the stop sequence that was used (if applicable).
        /// </summary>
        public string StopSequence { get; set; } = string.Empty;
    }
}
