using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using LLM.Nexus.Settings;
using Xunit;

namespace LLM.Nexus.Tests.Settings
{
    public class ProviderConfigurationTests
    {
        [Fact]
        public void ProviderConfiguration_WithAllRequiredFields_PassesValidation()
        {
            // Arrange
            var config = new ProviderConfiguration
            {
                Provider = LLMProvider.OpenAI,
                ApiKey = "test-api-key",
                Model = "gpt-4"
            };

            // Act
            var validationContext = new ValidationContext(config);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(config, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeTrue();
            validationResults.Should().BeEmpty();
        }

        [Fact]
        public void ProviderConfiguration_WithMissingApiKey_FailsValidation()
        {
            // Arrange
            var config = new ProviderConfiguration
            {
                Provider = LLMProvider.OpenAI,
                Model = "gpt-4"
            };

            // Act
            var validationContext = new ValidationContext(config);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(config, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().ContainSingle();
        }

        [Fact]
        public void ProviderConfiguration_WithMissingModel_FailsValidation()
        {
            // Arrange
            var config = new ProviderConfiguration
            {
                Provider = LLMProvider.OpenAI,
                ApiKey = "test-api-key"
            };

            // Act
            var validationContext = new ValidationContext(config);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(config, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().ContainSingle();
        }

        [Fact]
        public void MaxTokens_DefaultsTo2000()
        {
            // Arrange & Act
            var config = new ProviderConfiguration();

            // Assert
            config.MaxTokens.Should().Be(2000);
        }

        [Fact]
        public void MaxTokens_CanBeSetToCustomValue()
        {
            // Arrange & Act
            var config = new ProviderConfiguration
            {
                MaxTokens = 5000
            };

            // Assert
            config.MaxTokens.Should().Be(5000);
        }

        [Fact]
        public void Stream_CanBeSet()
        {
            // Arrange & Act
            var config = new ProviderConfiguration
            {
                Stream = true
            };

            // Assert
            config.Stream.Should().BeTrue();
        }

        [Fact]
        public void ProviderConfiguration_ForOpenAI_IsValid()
        {
            // Arrange & Act
            var config = new ProviderConfiguration
            {
                Provider = LLMProvider.OpenAI,
                ApiKey = "sk-test123",
                Model = "gpt-4",
                MaxTokens = 2000
            };

            // Assert
            config.Provider.Should().Be(LLMProvider.OpenAI);
            config.ApiKey.Should().Be("sk-test123");
            config.Model.Should().Be("gpt-4");
            config.MaxTokens.Should().Be(2000);
        }

        [Fact]
        public void ProviderConfiguration_ForAnthropic_IsValid()
        {
            // Arrange & Act
            var config = new ProviderConfiguration
            {
                Provider = LLMProvider.Anthropic,
                ApiKey = "sk-ant-test123",
                Model = "claude-sonnet-4-5-20250929",
                MaxTokens = 4000
            };

            // Assert
            config.Provider.Should().Be(LLMProvider.Anthropic);
            config.ApiKey.Should().Be("sk-ant-test123");
            config.Model.Should().Be("claude-sonnet-4-5-20250929");
            config.MaxTokens.Should().Be(4000);
        }

        [Fact]
        public void ProviderConfiguration_ForGoogle_IsValid()
        {
            // Arrange & Act
            var config = new ProviderConfiguration
            {
                Provider = LLMProvider.Google,
                ApiKey = "google-test123",
                Model = "gemini-2.0-flash",
                MaxTokens = 8000
            };

            // Assert
            config.Provider.Should().Be(LLMProvider.Google);
            config.ApiKey.Should().Be("google-test123");
            config.Model.Should().Be("gemini-2.0-flash");
            config.MaxTokens.Should().Be(8000);
        }
    }
}
