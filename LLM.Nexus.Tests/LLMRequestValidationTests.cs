using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using LLM.Nexus.Models;
using Xunit;

namespace LLM.Nexus.Tests
{
    public class LLMRequestValidationTests
    {
        [Fact]
        public void LLMRequest_WithSystemMessage_IsValid()
        {
            // Arrange
            var request = new LLMRequest
            {
                Prompt = "What is AI?",
                SystemMessage = "You are a helpful assistant"
            };

            // Act
            var validationContext = new ValidationContext(request);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeTrue();
            request.SystemMessage.Should().Be("You are a helpful assistant");
        }

        [Fact]
        public void LLMRequest_WithAdditionalParameters_CanBeSet()
        {
            // Arrange & Act
            var request = new LLMRequest
            {
                Prompt = "Test",
                AdditionalParameters = new Dictionary<string, object>
                {
                    { "top_p", 0.9 },
                    { "frequency_penalty", 0.5 }
                }
            };

            // Assert
            request.AdditionalParameters.Should().ContainKey("top_p");
            request.AdditionalParameters["top_p"].Should().Be(0.9);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(2.0)]
        public void LLMRequest_WithValidTemperature_PassesValidation(double temperature)
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
            isValid.Should().BeTrue();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        [InlineData(1000000)]
        public void LLMRequest_WithValidMaxTokens_PassesValidation(int maxTokens)
        {
            // Arrange
            var request = new LLMRequest
            {
                Prompt = "Test",
                MaxTokens = maxTokens
            };

            // Act
            var validationContext = new ValidationContext(request);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeTrue();
        }

        [Fact]
        public void LLMRequest_WithZeroMaxTokens_FailsValidation()
        {
            // Arrange
            var request = new LLMRequest
            {
                Prompt = "Test",
                MaxTokens = 0
            };

            // Act
            var validationContext = new ValidationContext(request);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeFalse();
        }

        [Fact]
        public void LLMRequest_WithNegativeMaxTokens_FailsValidation()
        {
            // Arrange
            var request = new LLMRequest
            {
                Prompt = "Test",
                MaxTokens = -1
            };

            // Act
            var validationContext = new ValidationContext(request);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeFalse();
        }

        [Fact]
        public void LLMRequest_WithVeryLongPrompt_PassesValidation()
        {
            // Arrange
            var longPrompt = new string('a', 10000); // 10K characters
            var request = new LLMRequest
            {
                Prompt = longPrompt
            };

            // Act
            var validationContext = new ValidationContext(request);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeTrue();
        }

        [Fact]
        public void LLMRequest_WithWhitespaceOnlyPrompt_FailsValidation()
        {
            // Arrange
            var request = new LLMRequest
            {
                Prompt = "   "
            };

            // Act
            var validationContext = new ValidationContext(request);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeFalse();
        }
    }
}
