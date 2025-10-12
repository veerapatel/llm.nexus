namespace LLM.Nexus.Models
{
    /// <summary>
    /// Represents token usage information from an LLM request.
    /// </summary>
    public class UsageInfo
    {
        /// <summary>
        /// Gets or sets the number of tokens in the prompt.
        /// </summary>
        public int PromptTokens { get; set; }

        /// <summary>
        /// Gets or sets the number of tokens in the completion.
        /// </summary>
        public int CompletionTokens { get; set; }

        /// <summary>
        /// Gets or sets the total number of tokens used (prompt + completion).
        /// </summary>
        public int TotalTokens { get; set; }
    }
}
