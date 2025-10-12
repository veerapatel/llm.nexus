using FluentAssertions;
using LLM.Nexus.Settings;
using Xunit;

namespace LLM.Nexus.Tests.Settings
{
    public class LLMProviderTests
    {
        [Fact]
        public void LLMProvider_HasOpenAIValue()
        {
            // Act
            var provider = LLMProvider.OpenAI;

            // Assert
            provider.Should().Be(LLMProvider.OpenAI);
            ((int)provider).Should().Be(0);
        }

        [Fact]
        public void LLMProvider_HasAnthropicValue()
        {
            // Act
            var provider = LLMProvider.Anthropic;

            // Assert
            provider.Should().Be(LLMProvider.Anthropic);
            ((int)provider).Should().Be(1);
        }

        [Fact]
        public void LLMProvider_CanBeUsedInSwitch()
        {
            // Arrange
            var provider = LLMProvider.OpenAI;
            string result = null;

            // Act
            switch (provider)
            {
                case LLMProvider.OpenAI:
                    result = "OpenAI";
                    break;
                case LLMProvider.Anthropic:
                    result = "Anthropic";
                    break;
            }

            // Assert
            result.Should().Be("OpenAI");
        }
    }
}
