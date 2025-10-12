using System.ComponentModel.DataAnnotations;
using LLM.Nexus.Models;
using Xunit;

namespace LLM.Nexus.Tests
{
    public class LLMRequestTests
    {
        [Fact]
        public void LLMRequest_WithValidPrompt_ShouldPassValidation()
        {
            // Arrange
            var request = new LLMRequest
            {
                Prompt = "Test prompt"
            };

            // Act
            var validationContext = new ValidationContext(request);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);

            // Assert
            Assert.True(isValid);
            Assert.Empty(validationResults);
        }

        [Fact]
        public void LLMRequest_WithEmptyPrompt_ShouldFailValidation()
        {
            // Arrange
            var request = new LLMRequest
            {
                Prompt = ""
            };

            // Act
            var validationContext = new ValidationContext(request);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.NotEmpty(validationResults);
        }

        [Fact]
        public void LLMRequest_WithNullPrompt_ShouldFailValidation()
        {
            // Arrange
            var request = new LLMRequest
            {
                Prompt = null
            };

            // Act
            var validationContext = new ValidationContext(request);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.NotEmpty(validationResults);
        }

        [Fact]
        public void LLMRequest_WithMaxTokens_ShouldSetProperty()
        {
            // Arrange & Act
            var request = new LLMRequest
            {
                Prompt = "Test",
                MaxTokens = 1000
            };

            // Assert
            Assert.Equal(1000, request.MaxTokens);
        }

        [Fact]
        public void LLMRequest_WithTemperature_ShouldSetProperty()
        {
            // Arrange & Act
            var request = new LLMRequest
            {
                Prompt = "Test",
                Temperature = 0.7
            };

            // Assert
            Assert.Equal(0.7, request.Temperature);
        }

        [Theory]
        [InlineData(-0.1)]
        [InlineData(2.1)]
        public void LLMRequest_WithInvalidTemperature_ShouldFailValidation(double temperature)
        {
            // Arrange
            var request = new LLMRequest
            {
                Prompt = "Test",
                Temperature = temperature
            };

            // Act
            var validationContext = new ValidationContext(request);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.NotEmpty(validationResults);
        }
    }
}
