using LLM.Nexus.Models;
using Xunit;

namespace LLM.Nexus.Tests
{
    public class UsageInfoTests
    {
        [Fact]
        public void UsageInfo_CanSetAllProperties()
        {
            // Arrange & Act
            var usage = new UsageInfo
            {
                PromptTokens = 100,
                CompletionTokens = 200,
                TotalTokens = 300
            };

            // Assert
            Assert.Equal(100, usage.PromptTokens);
            Assert.Equal(200, usage.CompletionTokens);
            Assert.Equal(300, usage.TotalTokens);
        }

        [Fact]
        public void UsageInfo_DefaultValuesAreZero()
        {
            // Arrange & Act
            var usage = new UsageInfo();

            // Assert
            Assert.Equal(0, usage.PromptTokens);
            Assert.Equal(0, usage.CompletionTokens);
            Assert.Equal(0, usage.TotalTokens);
        }
    }
}
