using System;
using System.Threading;
using System.Threading.Tasks;
using LLM.Nexus.Models;
using LLM.Nexus.Providers.Anthropic;
using LLM.Nexus.Providers.OpenAI;

namespace LLM.Nexus
{
    internal class LLMServiceAdapter : ILLMService
    {
        private readonly ILLMService _service;

        public LLMServiceAdapter(IOpenAIService openAIService)
        {
            _service = openAIService ?? throw new ArgumentNullException(nameof(openAIService));
        }

        public LLMServiceAdapter(IAnthropicService anthropicService)
        {
            _service = anthropicService ?? throw new ArgumentNullException(nameof(anthropicService));
        }

        public Task<LLMResponse> GenerateResponseAsync(LLMRequest request, CancellationToken cancellationToken = default)
        {
            return _service.GenerateResponseAsync(request, cancellationToken);
        }

        public Task<LLMResponse> GenerateResponseAsync(string prompt, CancellationToken cancellationToken = default)
        {
            return _service.GenerateResponseAsync(prompt, cancellationToken);
        }
    }
}
