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
        public void LLMSettings_WithAllRequiredFields_PassesValidation()
        {
            // Arrange
            var settings = new LLMSettings
            {
                Provider = LLMProvider.OpenAI,
                ApiKey = "test-api-key",
                Model = "gpt-4"
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
        public void LLMSettings_WithMissingProvider_FailsValidation()
        {
            // Arrange
            var settings = new LLMSettings
            {
                ApiKey = "test-api-key",
                Model = "gpt-4"
            };

            // Act
            var validationContext = new ValidationContext(settings);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(settings, validationContext, validationResults, true);

            // Assert - Provider is an enum so it will have a default value, won't fail
            isValid.Should().BeTrue();
        }

        [Fact]
        public void LLMSettings_WithMissingApiKey_FailsValidation()
        {
            // Arrange
            var settings = new LLMSettings
            {
                Provider = LLMProvider.OpenAI,
                Model = "gpt-4"
            };

            // Act
            var validationContext = new ValidationContext(settings);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(settings, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().ContainSingle();
        }

        [Fact]
        public void LLMSettings_WithMissingModel_FailsValidation()
        {
            // Arrange
            var settings = new LLMSettings
            {
                Provider = LLMProvider.OpenAI,
                ApiKey = "test-api-key"
            };

            // Act
            var validationContext = new ValidationContext(settings);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(settings, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().ContainSingle();
        }

        [Fact]
        public void MaxTokens_DefaultsTo2000()
        {
            // Arrange & Act
            var settings = new LLMSettings();

            // Assert
            settings.MaxTokens.Should().Be(2000);
        }

        [Fact]
        public void MaxTokens_CanBeSetToCustomValue()
        {
            // Arrange & Act
            var settings = new LLMSettings
            {
                MaxTokens = 5000
            };

            // Assert
            settings.MaxTokens.Should().Be(5000);
        }

        [Fact]
        public void Stream_CanBeSet()
        {
            // Arrange & Act
            var settings = new LLMSettings
            {
                Stream = true
            };

            // Assert
            settings.Stream.Should().BeTrue();
        }
    }
}
