using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LLM.Nexus.Models;
using LLM.Nexus.Providers.Anthropic;
using LLM.Nexus.Providers.OpenAI;
using Moq;
using Xunit;

namespace LLM.Nexus.Tests
{
    public class LLMServiceAdapterTests
    {
        [Fact]
        public void Constructor_WithNullOpenAIService_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new LLMServiceAdapter((IOpenAIService)null));
        }

        [Fact]
        public void Constructor_WithNullAnthropicService_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new LLMServiceAdapter((IAnthropicService)null));
        }

        [Fact]
        public async Task GenerateResponseAsync_WithStringPrompt_CallsUnderlyingService()
        {
            // Arrange
            var mockOpenAIService = new Mock<IOpenAIService>();
            var expectedResponse = new LLMResponse
            {
                Content = "Test response",
                Model = "gpt-4",
                Provider = "OpenAI"
            };

            mockOpenAIService
                .Setup(s => s.GenerateResponseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var adapter = new LLMServiceAdapter(mockOpenAIService.Object);

            // Act
            var result = await adapter.GenerateResponseAsync("Test prompt");

            // Assert
            result.Should().BeSameAs(expectedResponse);
            mockOpenAIService.Verify(s => s.GenerateResponseAsync("Test prompt", CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task GenerateResponseAsync_WithRequest_CallsUnderlyingService()
        {
            // Arrange
            var mockAnthropicService = new Mock<IAnthropicService>();
            var request = new LLMRequest { Prompt = "Test prompt" };
            var expectedResponse = new LLMResponse
            {
                Content = "Test response",
                Model = "claude-3-opus",
                Provider = "Anthropic"
            };

            mockAnthropicService
                .Setup(s => s.GenerateResponseAsync(It.IsAny<LLMRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var adapter = new LLMServiceAdapter(mockAnthropicService.Object);

            // Act
            var result = await adapter.GenerateResponseAsync(request);

            // Assert
            result.Should().BeSameAs(expectedResponse);
            mockAnthropicService.Verify(s => s.GenerateResponseAsync(request, CancellationToken.None), Times.Once);
        }
    }
}
