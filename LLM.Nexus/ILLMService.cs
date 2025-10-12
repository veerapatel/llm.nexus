using System.Threading;
using System.Threading.Tasks;
using LLM.Nexus.Models;

namespace LLM.Nexus
{
    /// <summary>
    /// Defines the contract for LLM service implementations.
    /// </summary>
    public interface ILLMService
    {
        /// <summary>
        /// Generates a response from the LLM based on the provided request.
        /// </summary>
        /// <param name="request">The request containing the prompt and optional parameters.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation, containing the LLM response.</returns>
        Task<LLMResponse> GenerateResponseAsync(LLMRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates a response from the LLM based on a simple prompt string.
        /// </summary>
        /// <param name="prompt">The prompt text to send to the LLM.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation, containing the LLM response.</returns>
        Task<LLMResponse> GenerateResponseAsync(string prompt, CancellationToken cancellationToken = default);
    }
}
