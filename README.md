# LLM.Nexus

[![NuGet](https://img.shields.io/nuget/v/LLM.Nexus.svg)](https://www.nuget.org/packages/LLM.Nexus)
[![NuGet Downloads](https://img.shields.io/nuget/dt/LLM.Nexus.svg)](https://www.nuget.org/packages/LLM.Nexus)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Tests](https://img.shields.io/badge/tests-65%20passed-brightgreen.svg)]()
[![.NET Standard](https://img.shields.io/badge/.NET%20Standard-2.0-blue.svg)]()

A unified .NET abstraction layer for multiple Large Language Model (LLM) providers. LLM.Nexus simplifies integration with OpenAI, Anthropic, Google, and other LLM services through a common interface, making it easy to switch between providers or support multiple providers in your application simultaneously.

## Features

- **Multi-Provider Support** - OpenAI (GPT), Anthropic (Claude), and Google (Gemini) with extensible architecture
- **Multiple Providers Simultaneously** - Configure and use multiple providers in the same application
- **Named Provider Instances** - Create and manage multiple configurations for different use cases
- **Unified Interface** - Single `ILLMService` interface works across all providers
- **Rich Response Metadata** - Access token usage, model information, finish reasons, and more
- **Dependency Injection** - First-class support for Microsoft.Extensions.DependencyInjection
- **Type-Safe Configuration** - Strongly-typed settings with validation on startup
- **Cancellation Support** - All async methods support cancellation tokens
- **Input Validation** - Built-in request validation with DataAnnotations
- **Comprehensive Logging** - Structured logging throughout the library
- **100% Test Coverage** - 65 passing tests covering all scenarios
- **.NET Standard 2.0** - Compatible with .NET Framework 4.6.1+ and .NET Core 2.0+

## Installation

```bash
dotnet add package LLM.Nexus
```

## Table of Contents

- [Quick Start](#quick-start)
- [Core Concepts](#core-concepts)
  - [LLM Service](#llm-service)
  - [Service Factory](#service-factory)
  - [Request and Response Models](#request-and-response-models)
- [Configuration](#configuration)
  - [Single Provider Configuration](#single-provider-configuration)
  - [Multi-Provider Configuration](#multi-provider-configuration)
  - [OpenAI Configuration](#openai-configuration)
  - [Anthropic Configuration](#anthropic-configuration)
  - [Google Configuration](#google-configuration)
- [Usage Examples](#usage-examples)
  - [Simple Usage](#simple-usage)
  - [Advanced Usage with LLMRequest](#advanced-usage-with-llmrequest)
  - [Cancellation Token Support](#cancellation-token-support)
  - [Accessing Response Metadata](#accessing-response-metadata)
- [Supported Models](#supported-models)
- [Error Handling](#error-handling)
- [Best Practices](#best-practices)
- [Architecture](#architecture)
- [Testing](#testing)
- [API Reference](#api-reference)
- [Contributing](#contributing)
- [License](#license)

## Quick Start

### 1. Configure Settings

Add LLM settings to your `appsettings.json`:

**Single Provider (Simple):**

```json
{
  "LLMSettings": {
    "Providers": {
      "default": {
        "Provider": "OpenAI",
        "ApiKey": "your-api-key-here",
        "Model": "gpt-4",
        "MaxTokens": 2000
      }
    }
  }
}
```

**Multiple Providers (Advanced):**

```json
{
  "LLMSettings": {
    "DefaultProvider": "openai-gpt4",
    "Providers": {
      "openai-gpt4": {
        "Provider": "OpenAI",
        "ApiKey": "sk-...",
        "Model": "gpt-4",
        "MaxTokens": 2000
      },
      "anthropic-claude": {
        "Provider": "Anthropic",
        "ApiKey": "sk-ant-...",
        "Model": "claude-sonnet-4-5-20250929",
        "MaxTokens": 4000
      },
      "google-gemini": {
        "Provider": "Google",
        "ApiKey": "...",
        "Model": "gemini-2.0-flash",
        "MaxTokens": 8000
      }
    }
  }
}
```

### 2. Register Services

In your `Program.cs` or `Startup.cs`:

```csharp
using LLM.Nexus;

var builder = WebApplication.CreateBuilder(args);

// Registers all LLM services with dependency injection
builder.Services.AddLLMServices();
```

### 3. Use the Service

```csharp
using LLM.Nexus;
using LLM.Nexus.Models;

public class MyService
{
    private readonly ILLMService _llmService;

    public MyService(ILLMServiceFactory factory)
    {
        _llmService = factory.CreateService();
    }

    public async Task<string> GenerateResponseAsync(string userPrompt)
    {
        var response = await _llmService.GenerateResponseAsync(userPrompt);

        Console.WriteLine($"Content: {response.Content}");
        Console.WriteLine($"Model: {response.Model}");
        Console.WriteLine($"Tokens Used: {response.Usage.TotalTokens}");
        Console.WriteLine($"Finish Reason: {response.FinishReason}");

        return response.Content;
    }
}
```

## Core Concepts

### LLM Service

The `ILLMService` interface provides a unified API for interacting with different LLM providers:

```csharp
public interface ILLMService
{
    Task<LLMResponse> GenerateResponseAsync(LLMRequest request, CancellationToken cancellationToken = default);
    Task<LLMResponse> GenerateResponseAsync(string prompt, CancellationToken cancellationToken = default);
}
```

Each provider (OpenAI, Anthropic) implements this interface, allowing you to switch providers without changing your application code.

### Service Factory

The `ILLMServiceFactory` creates the appropriate service instance based on your configuration:

```csharp
public interface ILLMServiceFactory
{
    ILLMService CreateService();
    ILLMService CreateService(string providerName);
    IEnumerable<string> GetConfiguredProviders();
    string GetDefaultProviderName();
}
```

This factory pattern enables:
- **Configuration-based provider selection** - Choose provider via settings
- **Multiple provider support** - Create services for different providers by name
- **Provider abstraction** - Your code doesn't need to know which provider is being used
- **Provider discovery** - Enumerate all configured providers
- **Easy testing** - Mock the factory in unit tests

### Request and Response Models

**LLMRequest** - Encapsulates all request parameters with validation:

```csharp
public class LLMRequest
{
    [Required]
    [StringLength(1000000, MinimumLength = 1)]
    public string Prompt { get; set; }

    public string? SystemMessage { get; set; }

    [Range(1, 100000)]
    public int? MaxTokens { get; set; }

    [Range(0.0, 2.0)]
    public double? Temperature { get; set; }

    public Dictionary<string, object>? AdditionalParameters { get; set; }
}
```

**LLMResponse** - Contains response data and comprehensive metadata:

```csharp
public class LLMResponse
{
    public string Id { get; set; }              // Unique response identifier
    public string Provider { get; set; }        // Provider name (OpenAI, Anthropic)
    public string Model { get; set; }           // Model used for generation
    public string Content { get; set; }         // Generated text content
    public DateTime Timestamp { get; set; }     // Response timestamp
    public string FinishReason { get; set; }    // Why generation stopped
    public TokenUsage Usage { get; set; }       // Token consumption details
}
```

## Configuration

### Configuration Options

LLM.Nexus supports both single and multi-provider configurations:

**LLMSettings:**

| Property | Type | Required | Default | Description |
|----------|------|----------|---------|-------------|
| `Providers` | `Dictionary<string, ProviderConfiguration>` | Yes | - | Named provider configurations |
| `DefaultProvider` | `string` | No | First provider | Default provider to use |

**ProviderConfiguration:**

| Property | Type | Required | Default | Description |
|----------|------|----------|---------|-------------|
| `Provider` | `LLMProvider` | Yes | - | The LLM provider (OpenAI, Anthropic, Google) |
| `ApiKey` | `string` | Yes | - | API key for authentication |
| `Model` | `string` | Yes | - | Model identifier |
| `MaxTokens` | `int?` | No | 2000 | Maximum tokens to generate |
| `Stream` | `bool?` | No | false | Enable streaming responses (future) |

### Single Provider Configuration

For applications using only one provider:

```json
{
  "LLMSettings": {
    "Providers": {
      "default": {
        "Provider": "OpenAI",
        "ApiKey": "sk-...",
        "Model": "gpt-4",
        "MaxTokens": 2000
      }
    }
  }
}
```

### Multi-Provider Configuration

For applications using multiple providers:

```json
{
  "LLMSettings": {
    "DefaultProvider": "openai",
    "Providers": {
      "openai": {
        "Provider": "OpenAI",
        "ApiKey": "sk-...",
        "Model": "gpt-4",
        "MaxTokens": 2000
      },
      "anthropic": {
        "Provider": "Anthropic",
        "ApiKey": "sk-ant-...",
        "Model": "claude-sonnet-4-5-20250929",
        "MaxTokens": 4000
      },
      "google": {
        "Provider": "Google",
        "ApiKey": "...",
        "Model": "gemini-2.0-flash",
        "MaxTokens": 8000
      }
    }
  }
}
```

### OpenAI Configuration

```json
{
  "LLMSettings": {
    "Providers": {
      "openai": {
        "Provider": "OpenAI",
        "ApiKey": "sk-...",
        "Model": "gpt-4",
        "MaxTokens": 2000
      }
    }
  }
}
```

**Supported Models:**
- `gpt-4` - Most capable, best for complex tasks
- `gpt-4-turbo` - Fast and capable, optimized for speed
- `gpt-3.5-turbo` - Fast and economical
- Other OpenAI chat models

### Anthropic Configuration

```json
{
  "LLMSettings": {
    "Providers": {
      "anthropic": {
        "Provider": "Anthropic",
        "ApiKey": "sk-ant-...",
        "Model": "claude-sonnet-4-5-20250929",
        "MaxTokens": 4000
      }
    }
  }
}
```

**Supported Models:**
- `claude-sonnet-4-5-20250929` - Latest Sonnet, balanced performance
- `claude-3-opus` - Most capable, best for complex tasks
- `claude-3-sonnet` - Balanced performance and speed
- `claude-3-haiku` - Fast and economical
- Other Claude models

### Google Configuration

```json
{
  "LLMSettings": {
    "Providers": {
      "google": {
        "Provider": "Google",
        "ApiKey": "your-google-ai-studio-api-key",
        "Model": "gemini-2.0-flash",
        "MaxTokens": 8000
      }
    }
  }
}
```

**Authentication:**
Google provider uses Google AI Studio API keys. Get your API key from [Google AI Studio](https://makersuite.google.com/app/apikey).

**Supported Models:**
- `gemini-2.0-flash` - Latest Flash model, fast and efficient
- `gemini-1.5-pro` - Production-ready model for various tasks
- `gemini-1.5-flash` - Fast and capable
- Other Gemini models (model name can include or exclude "models/" prefix)

### Environment Variable Configuration

For production environments, use environment variables or secure configuration:

```bash
# Using environment variables for single provider
export LLMSettings__Providers__default__Provider="OpenAI"
export LLMSettings__Providers__default__ApiKey="sk-..."
export LLMSettings__Providers__default__Model="gpt-4"

# For multiple providers
export LLMSettings__DefaultProvider="openai"
export LLMSettings__Providers__openai__Provider="OpenAI"
export LLMSettings__Providers__openai__ApiKey="sk-..."
export LLMSettings__Providers__openai__Model="gpt-4"
export LLMSettings__Providers__anthropic__Provider="Anthropic"
export LLMSettings__Providers__anthropic__ApiKey="sk-ant-..."
export LLMSettings__Providers__anthropic__Model="claude-sonnet-4-5-20250929"
```

```csharp
// Or use Azure Key Vault, AWS Secrets Manager, etc.
builder.Configuration.AddAzureKeyVault(/* ... */);
```

## Usage Examples

### Simple Usage

The simplest way to use LLM.Nexus with a string prompt:

```csharp
public class ChatService
{
    private readonly ILLMService _llmService;

    public ChatService(ILLMServiceFactory factory)
    {
        _llmService = factory.CreateService();
    }

    public async Task<string> AskQuestionAsync(string question)
    {
        var response = await _llmService.GenerateResponseAsync(question);
        return response.Content;
    }
}

// Usage
var answer = await chatService.AskQuestionAsync("What is quantum computing?");
```

### Advanced Usage with LLMRequest

Use `LLMRequest` for fine-grained control over generation parameters:

```csharp
public async Task<LLMResponse> GenerateCreativeStoryAsync()
{
    var request = new LLMRequest
    {
        Prompt = "Write a short story about a robot learning to paint",
        SystemMessage = "You are a creative writing assistant specializing in science fiction",
        MaxTokens = 1000,
        Temperature = 0.9  // Higher temperature for more creative output
    };

    return await _llmService.GenerateResponseAsync(request);
}
```

### Cancellation Token Support

All async methods support cancellation for long-running operations:

```csharp
public async Task<string> GenerateWithTimeoutAsync(string prompt)
{
    using var cts = new CancellationTokenSource();
    cts.CancelAfter(TimeSpan.FromSeconds(30));

    try
    {
        var response = await _llmService.GenerateResponseAsync(prompt, cts.Token);
        return response.Content;
    }
    catch (OperationCanceledException)
    {
        _logger.LogWarning("Request was cancelled after 30 seconds");
        return "Request timed out. Please try again.";
    }
}
```

### Accessing Response Metadata

LLM.Nexus provides comprehensive metadata about each response:

```csharp
public async Task AnalyzeResponseMetadataAsync(string prompt)
{
    var response = await _llmService.GenerateResponseAsync(prompt);

    // Response identification
    Console.WriteLine($"Response ID: {response.Id}");
    Console.WriteLine($"Provider: {response.Provider}");
    Console.WriteLine($"Model: {response.Model}");
    Console.WriteLine($"Timestamp: {response.Timestamp}");

    // Generation details
    Console.WriteLine($"Finish Reason: {response.FinishReason}");
    // Possible values: "stop", "length", "content_filter", etc.

    // Token usage (important for cost tracking)
    Console.WriteLine($"Prompt Tokens: {response.Usage.PromptTokens}");
    Console.WriteLine($"Completion Tokens: {response.Usage.CompletionTokens}");
    Console.WriteLine($"Total Tokens: {response.Usage.TotalTokens}");

    // Calculate approximate cost (example for GPT-4)
    decimal cost = CalculateCost(response.Usage, "gpt-4");
    Console.WriteLine($"Estimated Cost: ${cost:F4}");
}

private decimal CalculateCost(TokenUsage usage, string model)
{
    // Example pricing (adjust based on actual provider pricing)
    return model switch
    {
        "gpt-4" => (usage.PromptTokens * 0.03m / 1000) + (usage.CompletionTokens * 0.06m / 1000),
        "gpt-3.5-turbo" => (usage.TotalTokens * 0.002m / 1000),
        _ => 0
    };
}
```

### Provider-Specific Parameters

Use `AdditionalParameters` for provider-specific features:

```csharp
var request = new LLMRequest
{
    Prompt = "Explain neural networks",
    AdditionalParameters = new Dictionary<string, object>
    {
        { "top_p", 0.9 },
        { "frequency_penalty", 0.5 },
        { "presence_penalty", 0.5 }
    }
};

var response = await _llmService.GenerateResponseAsync(request);
```

### Multiple Providers in One Application

Use different providers for different use cases in the same application:

```csharp
public class MultiProviderService
{
    private readonly ILLMService _openAI;
    private readonly ILLMService _anthropic;
    private readonly ILLMService _google;
    private readonly ILLMServiceFactory _factory;

    public MultiProviderService(ILLMServiceFactory factory)
    {
        _factory = factory;

        // Create instances for each provider
        _openAI = factory.CreateService("openai-gpt4");
        _anthropic = factory.CreateService("anthropic-claude");
        _google = factory.CreateService("google-gemini");
    }

    public async Task<string> UseOpenAIAsync(string prompt)
    {
        var response = await _openAI.GenerateResponseAsync(prompt);
        return response.Content;
    }

    public async Task<string> UseAnthropicAsync(string prompt)
    {
        var response = await _anthropic.GenerateResponseAsync(prompt);
        return response.Content;
    }

    public async Task<string> UseGoogleAsync(string prompt)
    {
        var response = await _google.GenerateResponseAsync(prompt);
        return response.Content;
    }

    public async Task<string> CompareProvidersAsync(string prompt)
    {
        // Call all providers simultaneously
        var tasks = new[]
        {
            _openAI.GenerateResponseAsync(prompt),
            _anthropic.GenerateResponseAsync(prompt),
            _google.GenerateResponseAsync(prompt)
        };

        var responses = await Task.WhenAll(tasks);

        return $"OpenAI: {responses[0].Content}\n\n" +
               $"Anthropic: {responses[1].Content}\n\n" +
               $"Google: {responses[2].Content}";
    }

    public async Task<string> UseDefaultProviderAsync(string prompt)
    {
        // Uses the configured default provider
        var defaultService = _factory.CreateService();
        var response = await defaultService.GenerateResponseAsync(prompt);
        return response.Content;
    }

    public void ListConfiguredProviders()
    {
        var providers = _factory.GetConfiguredProviders();
        var defaultProvider = _factory.GetDefaultProviderName();

        Console.WriteLine($"Configured providers: {string.Join(", ", providers)}");
        Console.WriteLine($"Default provider: {defaultProvider}");
    }
}
```

## Supported Models

### OpenAI Models

| Model | Context Window | Best For | Speed | Cost |
|-------|----------------|----------|-------|------|
| `gpt-4` | 8K tokens | Complex reasoning, analysis | Slower | Higher |
| `gpt-4-turbo` | 128K tokens | Long documents, complex tasks | Fast | Medium |
| `gpt-3.5-turbo` | 16K tokens | Simple tasks, chat | Fastest | Lowest |

### Anthropic Models

| Model | Context Window | Best For | Speed | Cost |
|-------|----------------|----------|-------|------|
| `claude-sonnet-4-5-20250929` | 200K tokens | Latest features, balanced | Fast | Medium |
| `claude-3-opus` | 200K tokens | Complex reasoning | Medium | Higher |
| `claude-3-sonnet` | 200K tokens | Balanced performance | Fast | Medium |
| `claude-3-haiku` | 200K tokens | Simple tasks, speed | Fastest | Lowest |

### Google Models

| Model | Context Window | Best For | Speed | Cost |
|-------|----------------|----------|-------|------|
| `gemini-2.0-flash` | 1M tokens | Fast responses, efficiency | Fastest | Low |
| `gemini-1.5-pro` | 2M tokens | Production tasks, balanced | Fast | Medium |
| `gemini-1.5-flash` | 1M tokens | Fast and capable | Fastest | Low |

## Error Handling

LLM.Nexus throws specific exceptions for different error scenarios:

### Common Exceptions

```csharp
public async Task<string> HandleErrorsAsync(string prompt)
{
    try
    {
        var response = await _llmService.GenerateResponseAsync(prompt);
        return response.Content;
    }
    catch (ArgumentNullException ex)
    {
        // Required parameter is null
        _logger.LogError(ex, "Required parameter is missing");
        throw;
    }
    catch (ValidationException ex)
    {
        // Request validation failed (invalid prompt, temperature, etc.)
        _logger.LogError(ex, "Request validation failed: {Message}", ex.Message);
        return "Invalid request parameters. Please check your input.";
    }
    catch (HttpRequestException ex)
    {
        // Network or API error
        _logger.LogError(ex, "API request failed");
        return "Service temporarily unavailable. Please try again.";
    }
    catch (OperationCanceledException)
    {
        // Request was cancelled
        _logger.LogWarning("Request was cancelled by user or timeout");
        return "Request cancelled.";
    }
    catch (Exception ex)
    {
        // Unexpected error
        _logger.LogError(ex, "Unexpected error occurred");
        throw;
    }
}
```

### Validation Errors

The library validates all requests using DataAnnotations:

```csharp
var request = new LLMRequest
{
    Prompt = "",  // Invalid: too short
    Temperature = 3.0  // Invalid: out of range [0.0-2.0]
};

// Throws ValidationException with detailed error messages
try
{
    await _llmService.GenerateResponseAsync(request);
}
catch (ValidationException ex)
{
    // ex.Message contains all validation errors
    Console.WriteLine(ex.Message);
}
```

### Retry Logic

Implement retry logic for transient failures:

```csharp
public async Task<LLMResponse> GenerateWithRetryAsync(string prompt, int maxRetries = 3)
{
    var retryCount = 0;

    while (retryCount < maxRetries)
    {
        try
        {
            return await _llmService.GenerateResponseAsync(prompt);
        }
        catch (HttpRequestException ex) when (retryCount < maxRetries - 1)
        {
            retryCount++;
            var delay = TimeSpan.FromSeconds(Math.Pow(2, retryCount));

            _logger.LogWarning(
                ex,
                "Request failed, retrying in {Delay}s (attempt {Attempt}/{MaxRetries})",
                delay.TotalSeconds,
                retryCount,
                maxRetries);

            await Task.Delay(delay);
        }
    }

    throw new InvalidOperationException($"Failed after {maxRetries} retries");
}
```

## Best Practices

### Do

- **Store API keys securely** - Use environment variables, Azure Key Vault, or AWS Secrets Manager
- **Monitor token usage** - Track `response.Usage.TotalTokens` to manage costs
- **Use cancellation tokens** - Always pass cancellation tokens for long-running operations
- **Reuse service instances** - Services are registered as singletons and reuse HTTP clients
- **Validate inputs** - Use `LLMRequest` with DataAnnotations for robust validation
- **Log appropriately** - Use structured logging to track requests and errors
- **Handle rate limits** - Implement retry logic with exponential backoff
- **Choose the right model** - Balance cost, speed, and capability for your use case
- **Set reasonable timeouts** - Prevent requests from hanging indefinitely
- **Cache responses** - Cache responses when appropriate to reduce costs and latency

### Don't

- **Don't hardcode API keys** - Always use secure configuration
- **Don't ignore token limits** - Monitor and set appropriate `MaxTokens` values
- **Don't skip error handling** - Always handle exceptions appropriately
- **Don't create services manually** - Use the factory pattern and DI
- **Don't forget cancellation** - Support cancellation for better user experience
- **Don't log sensitive data** - Be careful not to log prompts containing PII
- **Don't ignore finish reasons** - Check `FinishReason` to detect truncated responses
- **Don't use high temperature for factual tasks** - Lower temperature (0.1-0.3) for accuracy

### Configuration Best Practices

```csharp
// Good: Secure configuration
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Load from secure sources
        services.AddLLMServices();

        // Configure validation
        services.AddOptions<LLMSettings>()
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }
}

// Good: Dependency injection
public class MyService
{
    private readonly ILLMService _llmService;

    public MyService(ILLMServiceFactory factory)
    {
        _llmService = factory.CreateService();
    }
}

// Bad: Manual instantiation
public class BadService
{
    public async Task DoWork()
    {
        // Don't do this!
        var service = new OpenAIService(/* ... */);
    }
}
```

## Architecture

### Design Overview

LLM.Nexus follows SOLID principles and clean architecture patterns:

```
┌──────────────────────────────────────────────────────────────┐
│                    Your Application                          │
└─────────────────────────┬────────────────────────────────────┘
                          │
                          ↓
┌──────────────────────────────────────────────────────────────┐
│           ILLMServiceFactory (Abstraction)                   │
│  CreateService() | CreateService(name)                       │
│  GetConfiguredProviders() | GetDefaultProviderName()         │
└─────────────────────────┬────────────────────────────────────┘
                          │
                          ↓
┌──────────────────────────────────────────────────────────────┐
│             ILLMService (Abstraction)                        │
│          GenerateResponseAsync(...)                          │
└───┬──────────────────────┬────────────────────┬──────────────┘
    │                      │                    │
┌───▼────────┐    ┌───────▼────────┐   ┌──────▼────────┐
│  OpenAI    │    │   Anthropic    │   │    Google     │
│  Service   │    │    Service     │   │   Service     │
│ (Provider) │    │   (Provider)   │   │  (Provider)   │
└───┬────────┘    └───────┬────────┘   └──────┬────────┘
    │                     │                    │
┌───▼────────┐    ┌───────▼────────┐   ┌──────▼────────┐
│  OpenAI    │    │   Anthropic    │   │   Google      │
│    SDK     │    │      SDK       │   │  Cloud SDK    │
│ (External) │    │   (External)   │   │  (External)   │
└────────────┘    └────────────────┘   └───────────────┘
```

### Key Components

**Abstractions Layer:**
- `ILLMService` - Unified service interface
- `ILLMServiceFactory` - Multi-provider service creation abstraction
- `LLMRequest` - Provider-agnostic request model
- `LLMResponse` - Provider-agnostic response model
- `ProviderConfiguration` - Individual provider settings

**Provider Layer:**
- `OpenAIService` - OpenAI implementation
- `AnthropicService` - Anthropic implementation
- `GoogleService` - Google Gemini implementation
- Provider-specific adapters and transformations

**Infrastructure Layer:**
- Dependency injection extensions with multi-provider support
- Configuration validation for multiple providers
- HTTP client management per provider
- Logging infrastructure with provider context

### Extensibility

Add new providers by implementing `ILLMService`:

```csharp
public class CustomProviderService : ILLMService
{
    public async Task<LLMResponse> GenerateResponseAsync(
        LLMRequest request,
        CancellationToken cancellationToken = default)
    {
        // Implement custom provider logic
        // Transform request to provider format
        // Call provider API
        // Transform response to LLMResponse

        return new LLMResponse
        {
            Id = "custom-id",
            Provider = "CustomProvider",
            Model = "custom-model",
            Content = "Generated content",
            Timestamp = DateTime.UtcNow,
            FinishReason = "stop",
            Usage = new TokenUsage
            {
                PromptTokens = 10,
                CompletionTokens = 20,
                TotalTokens = 30
            }
        };
    }

    public async Task<LLMResponse> GenerateResponseAsync(
        string prompt,
        CancellationToken cancellationToken = default)
    {
        return await GenerateResponseAsync(new LLMRequest { Prompt = prompt }, cancellationToken);
    }
}
```

## Testing

### Test Statistics

- **Total Tests**: 65
- **Passed**: 65
- **Failed**: 0
- **Code Coverage**: 100%
- **Test Framework**: xUnit

### Test Categories

| Category | Description | Test Count |
|----------|-------------|------------|
| Service Tests | Core service functionality, request/response handling | 18 |
| Factory Tests | Multi-provider factory, named provider creation | 15 |
| Configuration Tests | Multi-provider settings validation, configuration binding | 14 |
| Provider Tests | Provider-specific implementations (OpenAI, Anthropic, Google) | 12 |
| Integration Tests | Multi-provider scenarios, dependency injection | 6 |

### Running Tests

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test
dotnet test --filter "FullyQualifiedName~LLM.Nexus.Tests.ServiceTests"
```

### Example Test

```csharp
[Fact]
public async Task GenerateResponseAsync_WithValidPrompt_ReturnsResponse()
{
    // Arrange
    var factory = _serviceProvider.GetRequiredService<ILLMServiceFactory>();
    var service = factory.CreateService();
    var prompt = "What is 2+2?";

    // Act
    var response = await service.GenerateResponseAsync(prompt);

    // Assert
    Assert.NotNull(response);
    Assert.NotEmpty(response.Content);
    Assert.True(response.Usage.TotalTokens > 0);
    Assert.NotEmpty(response.Model);
}
```

### Mocking for Unit Tests

```csharp
public class MyServiceTests
{
    [Fact]
    public async Task MyMethod_CallsLLMService()
    {
        // Arrange
        var mockService = new Mock<ILLMService>();
        mockService
            .Setup(s => s.GenerateResponseAsync(It.IsAny<string>(), default))
            .ReturnsAsync(new LLMResponse
            {
                Content = "Mocked response",
                Usage = new TokenUsage { TotalTokens = 10 }
            });

        var mockFactory = new Mock<ILLMServiceFactory>();
        mockFactory.Setup(f => f.CreateService()).Returns(mockService.Object);

        var myService = new MyService(mockFactory.Object);

        // Act
        var result = await myService.GenerateResponseAsync("test");

        // Assert
        Assert.Equal("Mocked response", result);
        mockService.Verify(s => s.GenerateResponseAsync("test", default), Times.Once);
    }
}
```

## API Reference

### Core Interfaces

#### `ILLMService`

Main interface for LLM operations.

```csharp
public interface ILLMService
{
    Task<LLMResponse> GenerateResponseAsync(
        LLMRequest request,
        CancellationToken cancellationToken = default);

    Task<LLMResponse> GenerateResponseAsync(
        string prompt,
        CancellationToken cancellationToken = default);
}
```

#### `ILLMServiceFactory`

Factory for creating service instances with multi-provider support.

```csharp
public interface ILLMServiceFactory
{
    ILLMService CreateService();
    ILLMService CreateService(string providerName);
    IEnumerable<string> GetConfiguredProviders();
    string GetDefaultProviderName();
}
```

### Models

#### `LLMRequest`

Request parameters for LLM generation.

```csharp
public class LLMRequest
{
    [Required]
    [StringLength(1000000, MinimumLength = 1)]
    public string Prompt { get; set; }

    public string? SystemMessage { get; set; }

    [Range(1, 100000)]
    public int? MaxTokens { get; set; }

    [Range(0.0, 2.0)]
    public double? Temperature { get; set; }

    public Dictionary<string, object>? AdditionalParameters { get; set; }
}
```

#### `LLMResponse`

Response from LLM generation.

```csharp
public class LLMResponse
{
    public string Id { get; set; }
    public string Provider { get; set; }
    public string Model { get; set; }
    public string Content { get; set; }
    public DateTime Timestamp { get; set; }
    public string FinishReason { get; set; }
    public TokenUsage Usage { get; set; }
}
```

#### `TokenUsage`

Token consumption details.

```csharp
public class TokenUsage
{
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public int TotalTokens { get; set; }
}
```

#### `LLMSettings`

Configuration settings for multi-provider support.

```csharp
public class LLMSettings
{
    [Required]
    [MinLength(1)]
    public Dictionary<string, ProviderConfiguration> Providers { get; set; }

    public string DefaultProvider { get; set; }
}
```

#### `ProviderConfiguration`

Configuration for individual provider instances.

```csharp
public class ProviderConfiguration
{
    [Required]
    public LLMProvider Provider { get; set; }

    [Required]
    public string ApiKey { get; set; }

    [Required]
    public string Model { get; set; }

    public int? MaxTokens { get; set; } = 2000;

    public bool? Stream { get; set; }
}
```

#### `LLMProvider`

Supported providers enumeration.

```csharp
public enum LLMProvider
{
    OpenAI,
    Anthropic,
    Google
}
```

### Extension Methods

#### `AddLLMServices`

Registers LLM services with dependency injection.

```csharp
public static IServiceCollection AddLLMServices(this IServiceCollection services)
```

**Usage:**

```csharp
builder.Services.AddLLMServices();
```

**Registers:**
- `ILLMServiceFactory` as singleton with multi-provider support
- Configuration binding and validation
- HTTP clients with proper lifetime management

## Roadmap

- [x] Multi-provider support (v2.0)
- [x] Google Gemini support (v2.0)
- [x] Named provider instances (v2.0)
- [ ] Azure OpenAI support
- [ ] Streaming response support
- [ ] Function calling support
- [ ] Multi-turn conversation support
- [ ] Retry policies and circuit breakers
- [ ] Response caching
- [ ] Telemetry and metrics (OpenTelemetry)
- [ ] Rate limiting
- [ ] Token counting utilities
- [ ] Prompt templates
- [ ] Response validation
- [ ] Provider failover and load balancing
- [ ] Cost tracking and optimization

## Contributing

Contributions are welcome! This project maintains high standards:

- **100% test coverage required** - All new code must be fully tested
- **No breaking changes** - Follow semantic versioning
- **Documentation required** - Update README for new features
- **Follow existing patterns** - Maintain consistency with current architecture

### How to Contribute

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Write tests for your changes
4. Implement your changes
5. Ensure all tests pass (`dotnet test`)
6. Commit your changes (`git commit -m 'Add amazing feature'`)
7. Push to the branch (`git push origin feature/amazing-feature`)
8. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

**Copyright (c) 2025 Alexandros Mouratidis**

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

## Acknowledgments

- Built with [OpenAI .NET SDK](https://github.com/openai/openai-dotnet)
- Built with [Anthropic.SDK](https://github.com/tghamm/Anthropic.SDK)
- Built with [Google_GenerativeAI SDK](https://github.com/gunpal5/Google_GenerativeAI)

---

**Made with precision by Alexandros Mouratidis**
