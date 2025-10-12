using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LLM.Nexus.Models;
using LLM.Nexus.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LLM.Nexus.Tests
{
    public class IntegrationTests
    {
        [Fact]
        public void EndToEnd_OpenAIConfiguration_CreatesValidService()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder().Build();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddLogging();
            services.Configure<LLMSettings>(options =>
            {
                options.Provider = LLMProvider.OpenAI;
                options.ApiKey = "test-key-that-wont-be-used";
                options.Model = "gpt-4";
                options.MaxTokens = 1000;
            });
            services.AddLLMServices();

            var serviceProvider = services.BuildServiceProvider();

            // Act
            var factory = serviceProvider.GetRequiredService<ILLMServiceFactory>();
            var service = factory.CreateService();

            // Assert
            service.Should().NotBeNull();
            service.Should().BeAssignableTo<ILLMService>();
        }

        [Fact]
        public void EndToEnd_AnthropicConfiguration_CreatesValidService()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder().Build();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddLogging();
            services.Configure<LLMSettings>(options =>
            {
                options.Provider = LLMProvider.Anthropic;
                options.ApiKey = "test-key-that-wont-be-used";
                options.Model = "claude-3-opus";
                options.MaxTokens = 2000;
            });
            services.AddLLMServices();

            var serviceProvider = services.BuildServiceProvider();

            // Act
            var factory = serviceProvider.GetRequiredService<ILLMServiceFactory>();
            var service = factory.CreateService();

            // Assert
            service.Should().NotBeNull();
            service.Should().BeAssignableTo<ILLMService>();
        }

        [Fact]
        public async Task LLMRequest_WithCancellationToken_CanBeCancelled()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder().Build();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddLogging();
            services.Configure<LLMSettings>(options =>
            {
                options.Provider = LLMProvider.OpenAI;
                options.ApiKey = "sk-test-key";
                options.Model = "gpt-4";
            });
            services.AddLLMServices();

            var serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetRequiredService<ILLMServiceFactory>();
            var service = factory.CreateService();

            var cts = new CancellationTokenSource();
            cts.Cancel(); // Cancel immediately

            var request = new LLMRequest
            {
                Prompt = "Test prompt"
            };

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                await service.GenerateResponseAsync(request, cts.Token);
            });
        }

        [Fact]
        public async Task LLMRequest_WithNullRequest_ThrowsArgumentNullException()
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
            var factory = serviceProvider.GetRequiredService<ILLMServiceFactory>();
            var service = factory.CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await service.GenerateResponseAsync((LLMRequest)null);
            });
        }

        [Fact]
        public void ServiceLifetime_IsSingleton_ReturnsSameInstance()
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
            var factory1 = serviceProvider.GetRequiredService<ILLMServiceFactory>();
            var factory2 = serviceProvider.GetRequiredService<ILLMServiceFactory>();

            // Assert
            factory1.Should().BeSameAs(factory2, "services should be registered as singletons");
        }
    }
}
