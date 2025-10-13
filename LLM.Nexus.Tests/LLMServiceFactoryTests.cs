using System;
using FluentAssertions;
using LLM.Nexus.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace LLM.Nexus.Tests
{
    public class LLMServiceFactoryTests
    {
        [Fact]
        public void Constructor_WithNullLoggerFactory_ThrowsArgumentNullException()
        {
            // Arrange
            var settings = Options.Create(new LLMSettings
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
            });

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new LLMServiceFactory(null, settings));
        }

        [Fact]
        public void Constructor_WithNullSettings_ThrowsArgumentNullException()
        {
            // Arrange
            var loggerFactory = new Mock<ILoggerFactory>().Object;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new LLMServiceFactory(loggerFactory, null));
        }

        [Fact]
        public void Constructor_WithEmptyProviders_ThrowsInvalidOperationException()
        {
            // Arrange
            var loggerFactory = new Mock<ILoggerFactory>().Object;
            var settings = Options.Create(new LLMSettings
            {
                Providers = new Dictionary<string, ProviderConfiguration>()
            });

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => new LLMServiceFactory(loggerFactory, settings));
            exception.Message.Should().Contain("At least one provider must be configured");
        }

        [Fact]
        public void Constructor_WithInvalidDefaultProvider_ThrowsInvalidOperationException()
        {
            // Arrange
            var loggerFactory = new Mock<ILoggerFactory>().Object;
            var settings = Options.Create(new LLMSettings
            {
                DefaultProvider = "nonexistent",
                Providers = new Dictionary<string, ProviderConfiguration>
                {
                    ["openai"] = new ProviderConfiguration
                    {
                        Provider = LLMProvider.OpenAI,
                        ApiKey = "test-key",
                        Model = "gpt-4"
                    }
                }
            });

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => new LLMServiceFactory(loggerFactory, settings));
            exception.Message.Should().Contain("DefaultProvider");
        }

        [Fact]
        public void CreateService_WithDefaultProvider_ReturnsLLMService()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder().Build();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddLogging();
            services.AddLLMServices();
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

            var serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetRequiredService<ILLMServiceFactory>();

            // Act
            var service = factory.CreateService();

            // Assert
            service.Should().NotBeNull();
            service.Should().BeAssignableTo<ILLMService>();
        }

        [Fact]
        public void CreateService_WithSpecificProvider_ReturnsLLMService()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder().Build();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddLogging();
            services.AddLLMServices();
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
        public void CreateService_WithNonExistentProvider_ThrowsArgumentException()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder().Build();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddLogging();
            services.AddLLMServices();
            services.Configure<LLMSettings>(options =>
            {
                options.Providers = new Dictionary<string, ProviderConfiguration>
                {
                    ["openai"] = new ProviderConfiguration
                    {
                        Provider = LLMProvider.OpenAI,
                        ApiKey = "test-key",
                        Model = "gpt-4"
                    }
                };
            });

            var serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetRequiredService<ILLMServiceFactory>();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => factory.CreateService("nonexistent"));
            exception.Message.Should().Contain("not found in configuration");
        }

        [Fact]
        public void CreateService_WithEmptyProviderName_ThrowsArgumentException()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder().Build();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddLogging();
            services.AddLLMServices();
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

            var serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetRequiredService<ILLMServiceFactory>();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => factory.CreateService(""));
            Assert.Throws<ArgumentException>(() => factory.CreateService(null));
        }

        [Fact]
        public void GetConfiguredProviders_ReturnsAllProviderNames()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder().Build();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddLogging();
            services.AddLLMServices();
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
                    },
                    ["google"] = new ProviderConfiguration
                    {
                        Provider = LLMProvider.Google,
                        ApiKey = "test-key",
                        Model = "gemini-2.0-flash"
                    }
                };
            });

            var serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetRequiredService<ILLMServiceFactory>();

            // Act
            var providers = factory.GetConfiguredProviders();

            // Assert
            providers.Should().HaveCount(3);
            providers.Should().Contain("openai");
            providers.Should().Contain("anthropic");
            providers.Should().Contain("google");
        }

        [Fact]
        public void GetDefaultProviderName_WithExplicitDefault_ReturnsDefaultProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder().Build();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddLogging();
            services.AddLLMServices();
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

            var serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetRequiredService<ILLMServiceFactory>();

            // Act
            var defaultProvider = factory.GetDefaultProviderName();

            // Assert
            defaultProvider.Should().Be("anthropic");
        }

        [Fact]
        public void GetDefaultProviderName_WithoutExplicitDefault_ReturnsFirstProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder().Build();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddLogging();
            services.AddLLMServices();
            services.Configure<LLMSettings>(options =>
            {
                options.Providers = new Dictionary<string, ProviderConfiguration>
                {
                    ["openai"] = new ProviderConfiguration
                    {
                        Provider = LLMProvider.OpenAI,
                        ApiKey = "test-key",
                        Model = "gpt-4"
                    }
                };
            });

            var serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetRequiredService<ILLMServiceFactory>();

            // Act
            var defaultProvider = factory.GetDefaultProviderName();

            // Assert
            defaultProvider.Should().Be("openai");
        }

        [Fact]
        public void CreateService_WithInvalidProviderType_ThrowsArgumentException()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder().Build();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddLogging();
            services.AddLLMServices();
            services.Configure<LLMSettings>(options =>
            {
                options.Providers = new Dictionary<string, ProviderConfiguration>
                {
                    ["invalid"] = new ProviderConfiguration
                    {
                        Provider = (LLMProvider)999,
                        ApiKey = "test-key",
                        Model = "test-model"
                    }
                };
            });

            var serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetRequiredService<ILLMServiceFactory>();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => factory.CreateService("invalid"));
            exception.Message.Should().Contain("Unsupported LLM provider");
        }
    }
}
