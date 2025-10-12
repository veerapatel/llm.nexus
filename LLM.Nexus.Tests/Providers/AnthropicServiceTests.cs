using System;
using System.Threading.Tasks;
using FluentAssertions;
using LLM.Nexus.Models;
using LLM.Nexus.Providers.Anthropic;
using LLM.Nexus.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace LLM.Nexus.Tests.Providers
{
    public class AnthropicServiceTests
    {
        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Arrange
            var settings = Options.Create(new LLMSettings
            {
                Provider = LLMProvider.Anthropic,
                ApiKey = "test-key",
                Model = "claude-3-opus"
            });

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new AnthropicService(null, settings));
        }

        [Fact]
        public void Constructor_WithNullSettings_ThrowsArgumentNullException()
        {
            // Arrange
            var logger = new Mock<ILogger<AnthropicService>>().Object;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new AnthropicService(logger, null));
        }

        [Fact]
        public async Task GenerateResponseAsync_WithStringPrompt_CallsMainMethod()
        {
            // Arrange
            var logger = new Mock<ILogger<AnthropicService>>().Object;
            var settings = Options.Create(new LLMSettings
            {
                Provider = LLMProvider.Anthropic,
                ApiKey = "sk-ant-test-key",
                Model = "claude-3-opus"
            });

            var service = new AnthropicService(logger, settings);

            // Act & Assert - Should not throw validation exception
            try
            {
                await service.GenerateResponseAsync("Test prompt");
            }
            catch (Exception ex)
            {
                // We expect this to fail with API error since we're using fake API key
                // But it should reach the implementation, not fail at validation
                ex.Should().NotBeOfType<ArgumentNullException>();
            }
        }

        [Fact]
        public async Task GenerateResponseAsync_WithNullRequest_ThrowsArgumentNullException()
        {
            // Arrange
            var logger = new Mock<ILogger<AnthropicService>>().Object;
            var settings = Options.Create(new LLMSettings
            {
                Provider = LLMProvider.Anthropic,
                ApiKey = "test-key",
                Model = "claude-3-opus"
            });

            var service = new AnthropicService(logger, settings);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await service.GenerateResponseAsync((LLMRequest)null);
            });
        }

        [Fact]
        public async Task GenerateResponseAsync_WithRequestObject_ValidatesAndCallsAPI()
        {
            // Arrange
            var logger = new Mock<ILogger<AnthropicService>>().Object;
            var settings = Options.Create(new LLMSettings
            {
                Provider = LLMProvider.Anthropic,
                ApiKey = "sk-ant-test-key",
                Model = "claude-3-opus"
            });

            var service = new AnthropicService(logger, settings);
            var request = new LLMRequest
            {
                Prompt = "Test prompt",
                SystemMessage = "You are a helpful assistant",
                Temperature = 0.7,
                MaxTokens = 1000
            };

            // Act & Assert - Should not throw validation exception
            try
            {
                await service.GenerateResponseAsync(request);
            }
            catch (Exception ex)
            {
                // We expect this to fail with API error since we're using fake API key
                // But it should reach the implementation and handle the parameters
                ex.Should().NotBeOfType<ArgumentNullException>();
                ex.Should().NotBeOfType<System.ComponentModel.DataAnnotations.ValidationException>();
            }
        }
    }
}
