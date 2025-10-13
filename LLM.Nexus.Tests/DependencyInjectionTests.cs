using FluentAssertions;
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
        public void AddLLMServices_RegistersFactoryService()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder().Build();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddLogging();
            services.Configure<LLMSettings>(options =>
            {
                options.Providers = new Dictionary<string, ProviderConfiguration>
                {
                    ["default"] = new ProviderConfiguration
                    {
                        Provider = LLMProvider.OpenAI,
                        ApiKey = "test-key",
                        Model = "gpt-4"
                    }
                };
            });

            // Act
            services.AddLLMServices();
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var factory = serviceProvider.GetService<ILLMServiceFactory>();
            factory.Should().NotBeNull();
            factory.Should().BeOfType<LLMServiceFactory>();
        }

        [Fact]
        public void AddLLMServices_RegistersFactoryAsSingleton()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder().Build();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddLogging();
            services.Configure<LLMSettings>(options =>
            {
                options.Providers = new Dictionary<string, ProviderConfiguration>
                {
                    ["default"] = new ProviderConfiguration
                    {
                        Provider = LLMProvider.OpenAI,
                        ApiKey = "test-key",
                        Model = "gpt-4"
                    }
                };
            });

            services.AddLLMServices();
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var factory1 = serviceProvider.GetService<ILLMServiceFactory>();
            var factory2 = serviceProvider.GetService<ILLMServiceFactory>();

            // Assert
            factory1.Should().BeSameAs(factory2);
        }

        [Fact]
        public void AddLLMServices_ConfiguresOptionsValidation()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder().Build();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddLogging(); // Required for factory
            services.Configure<LLMSettings>(options =>
            {
                // Empty providers - should fail validation
                options.Providers = new Dictionary<string, ProviderConfiguration>();
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

        [Fact]
        public void AddLLMServices_WithMultipleProviders_AllowsFactoryToCreateServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder().Build();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddLogging();
            services.Configure<LLMSettings>(options =>
            {
                options.Providers = new Dictionary<string, ProviderConfiguration>
                {
                    ["openai"] = new ProviderConfiguration
                    {
                        Provider = LLMProvider.OpenAI,
                        ApiKey = "test-key",
                        Model = "gpt-4"
                    },
                    ["anthropic"] = new ProviderConfiguration
                    {
                        Provider = LLMProvider.Anthropic,
                        ApiKey = "test-key",
                        Model = "claude-sonnet-4-5-20250929"
                    }
                };
            });

            services.AddLLMServices();
            var serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetRequiredService<ILLMServiceFactory>();

            // Act
            var openAIService = factory.CreateService("openai");
            var anthropicService = factory.CreateService("anthropic");

            // Assert
            openAIService.Should().NotBeNull();
            openAIService.Should().BeAssignableTo<ILLMService>();
            anthropicService.Should().NotBeNull();
            anthropicService.Should().BeAssignableTo<ILLMService>();
        }

        [Fact]
        public void AddLLMServices_WithDefaultProvider_CreatesDefaultService()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder().Build();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddLogging();
            services.Configure<LLMSettings>(options =>
            {
                options.DefaultProvider = "anthropic";
                options.Providers = new Dictionary<string, ProviderConfiguration>
                {
                    ["openai"] = new ProviderConfiguration
                    {
                        Provider = LLMProvider.OpenAI,
                        ApiKey = "test-key",
                        Model = "gpt-4"
                    },
                    ["anthropic"] = new ProviderConfiguration
                    {
                        Provider = LLMProvider.Anthropic,
                        ApiKey = "test-key",
                        Model = "claude-sonnet-4-5-20250929"
                    }
                };
            });

            services.AddLLMServices();
            var serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetRequiredService<ILLMServiceFactory>();

            // Act
            var defaultService = factory.CreateService();
            var defaultProviderName = factory.GetDefaultProviderName();

            // Assert
            defaultService.Should().NotBeNull();
            defaultService.Should().BeAssignableTo<ILLMService>();
            defaultProviderName.Should().Be("anthropic");
        }
    }
}
