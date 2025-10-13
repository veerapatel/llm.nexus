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
        public void LLMProvider_HasGoogleValue()
        {
            // Act
            var provider = LLMProvider.Google;

            // Assert
            provider.Should().Be(LLMProvider.Google);
            ((int)provider).Should().Be(2);
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
                case LLMProvider.Google:
                    result = "Google";
                    break;
            }

            // Assert
            result.Should().Be("OpenAI");
        }

        [Fact]
        public void LLMProvider_AllProvidersCanBeUsedInSwitch()
        {
            // Test all providers work in switch
            var providers = new[] { LLMProvider.OpenAI, LLMProvider.Anthropic, LLMProvider.Google };
            var expectedResults = new[] { "OpenAI", "Anthropic", "Google" };

            for (int i = 0; i < providers.Length; i++)
            {
                string result = providers[i] switch
                {
                    LLMProvider.OpenAI => "OpenAI",
                    LLMProvider.Anthropic => "Anthropic",
                    LLMProvider.Google => "Google",
                    _ => "Unknown"
                };

                result.Should().Be(expectedResults[i]);
            }
        }
    }
}
