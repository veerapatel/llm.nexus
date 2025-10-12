using FluentAssertions;
using LLM.Nexus.Providers.Anthropic;
using LLM.Nexus.Providers.OpenAI;
using LLM.Nexus.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace LLM.Nexus.Tests
{
    public class DependencyInjectionTests
    {
        [Fact]
        public void AddLLMServices_RegistersAllRequiredServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder().Build();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddLogging();
            services.Configure<LLMSettings>(options =>
            {
                options.Provider = LLMProvider.OpenAI;
                options.ApiKey = "test-key";
                options.Model = "gpt-4";
            });

            // Act
            services.AddLLMServices();
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var factory = serviceProvider.GetService<ILLMServiceFactory>();
            factory.Should().NotBeNull();
            factory.Should().BeOfType<LLMServiceFactory>();

            var openAIService = serviceProvider.GetService<IOpenAIService>();
            openAIService.Should().NotBeNull();

            var anthropicService = serviceProvider.GetService<IAnthropicService>();
            anthropicService.Should().NotBeNull();
        }

        [Fact]
        public void AddLLMServices_RegistersServicesAsSingletons()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder().Build();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddLogging();
            services.Configure<LLMSettings>(options =>
            {
                options.Provider = LLMProvider.OpenAI;
                options.ApiKey = "test-key";
                options.Model = "gpt-4";
            });

            services.AddLLMServices();
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var factory1 = serviceProvider.GetService<ILLMServiceFactory>();
            var factory2 = serviceProvider.GetService<ILLMServiceFactory>();

            var openAI1 = serviceProvider.GetService<IOpenAIService>();
            var openAI2 = serviceProvider.GetService<IOpenAIService>();

            var anthropic1 = serviceProvider.GetService<IAnthropicService>();
            var anthropic2 = serviceProvider.GetService<IAnthropicService>();

            // Assert
            factory1.Should().BeSameAs(factory2);
            openAI1.Should().BeSameAs(openAI2);
            anthropic1.Should().BeSameAs(anthropic2);
        }

        [Fact]
        public void AddLLMServices_ConfiguresOptionsValidation()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder().Build();
            services.AddSingleton<IConfiguration>(configuration);
            services.Configure<LLMSettings>(options =>
            {
                // Missing required fields
                options.Provider = LLMProvider.OpenAI;
                // ApiKey and Model are missing
            });

            services.AddLLMServices();

            // Act & Assert
            var exception = Assert.Throws<OptionsValidationException>(() =>
            {
                var serviceProvider = services.BuildServiceProvider();
                _ = serviceProvider.GetRequiredService<ILLMServiceFactory>();
            });

            exception.Should().NotBeNull();
        }

        [Fact]
        public void AddLLMServices_ReturnsServiceCollection_ForChaining()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var result = services.AddLLMServices();

            // Assert
            result.Should().BeSameAs(services);
        }
    }
}
