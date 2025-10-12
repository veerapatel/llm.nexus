using System;
using System.Threading.Tasks;
using FluentAssertions;
using LLM.Nexus.Models;
using LLM.Nexus.Providers.OpenAI;
using LLM.Nexus.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace LLM.Nexus.Tests.Providers
{
    public class OpenAIServiceTests
    {
        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Arrange
            var settings = Options.Create(new LLMSettings
            {
                Provider = LLMProvider.OpenAI,
                ApiKey = "test-key",
                Model = "gpt-4"
            });

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new OpenAIService(null, settings));
        }

        [Fact]
        public void Constructor_WithNullSettings_ThrowsArgumentNullException()
        {
            // Arrange
            var logger = new Mock<ILogger<OpenAIService>>().Object;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new OpenAIService(logger, null));
        }

        [Fact]
        public async Task GenerateResponseAsync_WithStringPrompt_CallsMainMethod()
        {
            // Arrange
            var logger = new Mock<ILogger<OpenAIService>>().Object;
            var settings = Options.Create(new LLMSettings
            {
                Provider = LLMProvider.OpenAI,
                ApiKey = "sk-test-key",
                Model = "gpt-4"
            });

            var service = new OpenAIService(logger, settings);

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
    }
}
