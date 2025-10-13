using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using LLM.Nexus.Settings;
using Xunit;

namespace LLM.Nexus.Tests.Settings
{
    public class LLMSettingsTests
    {
        [Fact]
        public void Section_HasCorrectValue()
        {
            // Assert
            LLMSettings.Section.Should().Be("LLMSettings");
        }

        [Fact]
        public void LLMSettings_WithSingleProvider_PassesValidation()
        {
            // Arrange
            var settings = new LLMSettings
            {
                Providers = new Dictionary<string, ProviderConfiguration>
                {
                    ["default"] = new ProviderConfiguration
                    {
                        Provider = LLMProvider.OpenAI,
                        ApiKey = "test-api-key",
                        Model = "gpt-4"
                    }
                }
            };

            // Act
            var validationContext = new ValidationContext(settings);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(settings, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeTrue();
            validationResults.Should().BeEmpty();
        }

        [Fact]
        public void LLMSettings_WithMultipleProviders_PassesValidation()
        {
            // Arrange
            var settings = new LLMSettings
            {
                DefaultProvider = "openai",
                Providers = new Dictionary<string, ProviderConfiguration>
                {
                    ["openai"] = new ProviderConfiguration
                    {
                        Provider = LLMProvider.OpenAI,
                        ApiKey = "openai-key",
                        Model = "gpt-4"
                    },
                    ["anthropic"] = new ProviderConfiguration
                    {
                        Provider = LLMProvider.Anthropic,
                        ApiKey = "anthropic-key",
                        Model = "claude-sonnet-4-5-20250929"
                    }
                }
            };

            // Act
            var validationContext = new ValidationContext(settings);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(settings, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeTrue();
            validationResults.Should().BeEmpty();
        }

        [Fact]
        public void LLMSettings_WithEmptyProviders_FailsValidation()
        {
            // Arrange
            var settings = new LLMSettings
            {
                Providers = new Dictionary<string, ProviderConfiguration>()
            };

            // Act
            var validationContext = new ValidationContext(settings);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(settings, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().NotBeEmpty();
        }

        [Fact]
        public void LLMSettings_DefaultProvider_CanBeEmpty()
        {
            // Arrange & Act
            var settings = new LLMSettings
            {
                Providers = new Dictionary<string, ProviderConfiguration>
                {
                    ["default"] = new ProviderConfiguration
                    {
                        Provider = LLMProvider.OpenAI,
                        ApiKey = "test-key",
                        Model = "gpt-4"
                    }
                }
            };

            // Assert
            settings.DefaultProvider.Should().BeEmpty();
        }

        [Fact]
        public void LLMSettings_DefaultProvider_CanBeSet()
        {
            // Arrange & Act
            var settings = new LLMSettings
            {
                DefaultProvider = "openai",
                Providers = new Dictionary<string, ProviderConfiguration>
                {
                    ["openai"] = new ProviderConfiguration
                    {
                        Provider = LLMProvider.OpenAI,
                        ApiKey = "test-key",
                        Model = "gpt-4"
                    }
                }
            };

            // Assert
            settings.DefaultProvider.Should().Be("openai");
        }

        [Fact]
        public void LLMSettings_ProvidersCollection_IsInitialized()
        {
            // Arrange & Act
            var settings = new LLMSettings();

            // Assert
            settings.Providers.Should().NotBeNull();
            settings.Providers.Should().BeEmpty();
        }
    }
}
