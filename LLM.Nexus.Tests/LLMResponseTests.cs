using System;
using LLM.Nexus.Models;
using Xunit;

namespace LLM.Nexus.Tests
{
    public class LLMResponseTests
    {
        [Fact]
        public void LLMResponse_CanSetAllProperties()
        {
            // Arrange
            var timestamp = DateTimeOffset.UtcNow;
            var usage = new UsageInfo
            {
                PromptTokens = 10,
                CompletionTokens = 20,
                TotalTokens = 30
            };

            // Act
            var response = new LLMResponse
            {
                Content = "Test content",
                Id = "test-id-123",
                Model = "gpt-4",
                Provider = "OpenAI",
                Timestamp = timestamp,
                FinishReason = "stop",
                StopSequence = "seq",
                Usage = usage
            };

            // Assert
            Assert.Equal("Test content", response.Content);
            Assert.Equal("test-id-123", response.Id);
            Assert.Equal("gpt-4", response.Model);
            Assert.Equal("OpenAI", response.Provider);
            Assert.Equal(timestamp, response.Timestamp);
            Assert.Equal("stop", response.FinishReason);
            Assert.Equal("seq", response.StopSequence);
            Assert.NotNull(response.Usage);
            Assert.Equal(10, response.Usage.PromptTokens);
            Assert.Equal(20, response.Usage.CompletionTokens);
            Assert.Equal(30, response.Usage.TotalTokens);
        }
    }
}
