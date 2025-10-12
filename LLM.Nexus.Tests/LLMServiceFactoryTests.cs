using System;
using FluentAssertions;
using LLM.Nexus.Providers.Anthropic;
using LLM.Nexus.Providers.OpenAI;
using LLM.Nexus.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace LLM.Nexus.Tests
{
    public class LLMServiceFactoryTests
    {
        [Fact]
        public void Constructor_WithNullServiceProvider_ThrowsArgumentNullException()
        {
            // Arrange
            var settings = Options.Create(new LLMSettings
            {
                Provider = LLMProvider.OpenAI,
                ApiKey = "test-key",
                Model = "gpt-4"
            });

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new LLMServiceFactory(null, settings));
        }

        [Fact]
        public void Constructor_WithNullSettings_ThrowsArgumentNullException()
        {
            // Arrange
            var serviceProvider = new Mock<IServiceProvider>().Object;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new LLMServiceFactory(serviceProvider, null));
        }

        [Fact]
        public void CreateService_WithOpenAIProvider_ReturnsLLMService()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder().Build();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddLogging();
            services.AddLLMServices();
            services.Configure<LLMSettings>(options =>
            {
                options.Provider = LLMProvider.OpenAI;
                options.ApiKey = "test-key";
                options.Model = "gpt-4";
            });

            var serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetRequiredService<ILLMServiceFactory>();

            // Act
            var service = factory.CreateService();

            // Assert
            service.Should().NotBeNull();
            service.Should().BeAssignableTo<ILLMService>();
        }

        [Fact]
        public void CreateService_WithAnthropicProvider_ReturnsLLMService()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder().Build();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddLogging();
            services.AddLLMServices();
            services.Configure<LLMSettings>(options =>
            {
                options.Provider = LLMProvider.Anthropic;
                options.ApiKey = "test-key";
                options.Model = "claude-3-opus";
            });

            var serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetRequiredService<ILLMServiceFactory>();

            // Act
            var service = factory.CreateService();

            // Assert
            service.Should().NotBeNull();
            service.Should().BeAssignableTo<ILLMService>();
        }

        [Fact]
        public void CreateService_WithInvalidProvider_ThrowsArgumentException()
        {
            // Arrange
            var mockOpenAIService = new Mock<IOpenAIService>();
            var mockAnthropicService = new Mock<IAnthropicService>();
            var mockServiceProvider = new Mock<IServiceProvider>();

            mockServiceProvider.Setup(sp => sp.GetService(typeof(IOpenAIService)))
                .Returns(mockOpenAIService.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IAnthropicService)))
                .Returns(mockAnthropicService.Object);

            var settings = Options.Create(new LLMSettings
            {
                Provider = (LLMProvider)999, // Invalid provider
                ApiKey = "test-key",
                Model = "test-model"
            });

            var factory = new LLMServiceFactory(mockServiceProvider.Object, settings);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => factory.CreateService());
            exception.Message.Should().Contain("Unsupported LLM provider");
        }
    }
}
