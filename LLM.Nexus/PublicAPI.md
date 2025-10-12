# LLM.Nexus Public API Reference

**Version:** 1.0.0
**Target Framework:** .NET Standard 2.0
**Package:** [LLM.Nexus](https://www.nuget.org/packages/LLM.Nexus)

This document provides a comprehensive reference for all public APIs exposed by the LLM.Nexus library.

---

## Table of Contents

- [Namespaces](#namespaces)
- [Core Interfaces](#core-interfaces)
  - [ILLMService](#illmservice)
  - [ILLMServiceFactory](#illmservicefactory)
- [Models](#models)
  - [LLMRequest](#llmrequest)
  - [LLMResponse](#llmresponse)
  - [UsageInfo](#usageinfo)
- [Configuration](#configuration)
  - [LLMSettings](#llmsettings)
  - [LLMProvider](#llmprovider)
- [Dependency Injection](#dependency-injection)
  - [DependencyInjection](#dependencyinjection-class)
- [Extension Methods](#extension-methods)
- [Type Reference](#type-reference)

---

## Namespaces

The library exposes the following public namespaces:

| Namespace | Description |
|-----------|-------------|
| `LLM.Nexus` | Core interfaces and factory |
| `LLM.Nexus.Models` | Request and response models |
| `LLM.Nexus.Settings` | Configuration classes and enums |

---

## Core Interfaces

### ILLMService

**Namespace:** `LLM.Nexus`

**Description:** Defines the contract for LLM service implementations. This is the main interface for interacting with LLM providers.

#### Methods

##### GenerateResponseAsync(LLMRequest, CancellationToken)

Generates a response from the LLM based on the provided request.

```csharp
Task<LLMResponse> GenerateResponseAsync(
    LLMRequest request,
    CancellationToken cancellationToken = default
)
```

**Parameters:**
- `request` (`LLMRequest`): The request containing the prompt and optional parameters.
- `cancellationToken` (`CancellationToken`, optional): A cancellation token to cancel the operation.

**Returns:** `Task<LLMResponse>` - A task that represents the asynchronous operation, containing the LLM response.

**Exceptions:**
- `ArgumentNullException`: When `request` is null.
- `ValidationException`: When request validation fails.
- `HttpRequestException`: When the API request fails.
- `OperationCanceledException`: When the operation is cancelled.

**Example:**
```csharp
var request = new LLMRequest
{
    Prompt = "Explain quantum computing",
    MaxTokens = 500,
    Temperature = 0.7
};

var response = await llmService.GenerateResponseAsync(request);
Console.WriteLine(response.Content);
```

##### GenerateResponseAsync(string, CancellationToken)

Generates a response from the LLM based on a simple prompt string.

```csharp
Task<LLMResponse> GenerateResponseAsync(
    string prompt,
    CancellationToken cancellationToken = default
)
```

**Parameters:**
- `prompt` (`string`): The prompt text to send to the LLM.
- `cancellationToken` (`CancellationToken`, optional): A cancellation token to cancel the operation.

**Returns:** `Task<LLMResponse>` - A task that represents the asynchronous operation, containing the LLM response.

**Exceptions:**
- `ArgumentNullException`: When `prompt` is null.
- `ValidationException`: When prompt validation fails.
- `HttpRequestException`: When the API request fails.
- `OperationCanceledException`: When the operation is cancelled.

**Example:**
```csharp
var response = await llmService.GenerateResponseAsync("What is AI?");
Console.WriteLine(response.Content);
```

---

### ILLMServiceFactory

**Namespace:** `LLM.Nexus`

**Description:** Factory for creating LLM service instances based on configuration.

#### Methods

##### CreateService()

Creates an LLM service instance based on the configured provider.

```csharp
ILLMService CreateService()
```

**Returns:** `ILLMService` - An instance of `ILLMService` for the configured provider.

**Exceptions:**
- `InvalidOperationException`: When the configured provider is not supported.
- `ArgumentException`: When configuration is invalid.

**Example:**
```csharp
public class MyService
{
    private readonly ILLMService _llmService;

    public MyService(ILLMServiceFactory factory)
    {
        _llmService = factory.CreateService();
    }
}
```

---

## Models

### LLMRequest

**Namespace:** `LLM.Nexus.Models`

**Description:** Represents a request to an LLM provider with validation attributes.

#### Properties

| Property | Type | Required | Validation | Default | Description |
|----------|------|----------|------------|---------|-------------|
| `Prompt` | `string` | Yes | 1-1,000,000 chars | - | The prompt/user message to send to the LLM |
| `SystemMessage` | `string` | No | - | `null` | System message/instructions (optional) |
| `MaxTokens` | `int?` | No | 1-1,000,000 | `null` | Maximum number of tokens to generate |
| `Temperature` | `double?` | No | 0.0-2.0 | `null` | Temperature for response generation |
| `AdditionalParameters` | `Dictionary<string, object>` | No | - | `null` | Provider-specific parameters |

#### Validation Rules

- **Prompt**: Required, must be between 1 and 1,000,000 characters
- **MaxTokens**: If specified, must be between 1 and 1,000,000
- **Temperature**: If specified, must be between 0.0 and 2.0

#### Example

```csharp
var request = new LLMRequest
{
    Prompt = "Write a poem about the ocean",
    SystemMessage = "You are a creative poet",
    MaxTokens = 500,
    Temperature = 0.9,
    AdditionalParameters = new Dictionary<string, object>
    {
        { "top_p", 0.95 },
        { "frequency_penalty", 0.5 }
    }
};
```

---

### LLMResponse

**Namespace:** `LLM.Nexus.Models`

**Description:** Represents the response from an LLM provider with comprehensive metadata.

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Content` | `string` | The generated text content |
| `Id` | `string` | Unique identifier for this response from the provider |
| `Model` | `string` | The model that generated the response |
| `Provider` | `string` | The provider that generated the response |
| `Usage` | `UsageInfo` | Token usage information |
| `Timestamp` | `DateTimeOffset` | When the response was generated (UTC) |
| `FinishReason` | `string` | Why generation stopped (e.g., "stop", "length", "content_filter") |
| `StopSequence` | `string` | The stop sequence that was used (if applicable) |

#### Finish Reasons

Common finish reasons include:

| Finish Reason | Description |
|---------------|-------------|
| `stop` | Natural completion point reached |
| `length` | Maximum token limit reached |
| `content_filter` | Content was filtered by safety systems |
| `end_turn` | Turn ended naturally (Anthropic) |

#### Example

```csharp
var response = await llmService.GenerateResponseAsync("Hello!");

Console.WriteLine($"Response ID: {response.Id}");
Console.WriteLine($"Provider: {response.Provider}");
Console.WriteLine($"Model: {response.Model}");
Console.WriteLine($"Content: {response.Content}");
Console.WriteLine($"Tokens Used: {response.Usage.TotalTokens}");
Console.WriteLine($"Finish Reason: {response.FinishReason}");
Console.WriteLine($"Timestamp: {response.Timestamp}");
```

---

### UsageInfo

**Namespace:** `LLM.Nexus.Models`

**Description:** Represents token usage information from an LLM request.

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `PromptTokens` | `int` | Number of tokens in the prompt |
| `CompletionTokens` | `int` | Number of tokens in the completion |
| `TotalTokens` | `int` | Total number of tokens used (prompt + completion) |

#### Example

```csharp
var response = await llmService.GenerateResponseAsync("Explain AI");

Console.WriteLine($"Prompt Tokens: {response.Usage.PromptTokens}");
Console.WriteLine($"Completion Tokens: {response.Usage.CompletionTokens}");
Console.WriteLine($"Total Tokens: {response.Usage.TotalTokens}");

// Calculate approximate cost (example for GPT-4)
decimal cost = (response.Usage.PromptTokens * 0.03m / 1000) +
               (response.Usage.CompletionTokens * 0.06m / 1000);
Console.WriteLine($"Estimated Cost: ${cost:F4}");
```

---

## Configuration

### LLMSettings

**Namespace:** `LLM.Nexus.Settings`

**Description:** Configuration settings for LLM providers with validation attributes.

#### Properties

| Property | Type | Required | Default | Description |
|----------|------|----------|---------|-------------|
| `Provider` | `LLMProvider` | Yes | - | The LLM provider to use |
| `ApiKey` | `string` | Yes | - | API key for authentication |
| `Model` | `string` | Yes | - | Model identifier |
| `MaxTokens` | `int?` | No | 2000 | Maximum tokens to generate |
| `Stream` | `bool?` | No | `null` | Enable streaming responses (future) |

#### Constants

| Constant | Type | Value | Description |
|----------|------|-------|-------------|
| `Section` | `string` | `"LLMSettings"` | Configuration section name |

#### Configuration Example (appsettings.json)

```json
{
  "LLMSettings": {
    "Provider": "OpenAI",
    "ApiKey": "sk-...",
    "Model": "gpt-4",
    "MaxTokens": 2000
  }
}
```

#### Code Example

```csharp
// Access settings via IOptions
public class MyService
{
    private readonly LLMSettings _settings;

    public MyService(IOptions<LLMSettings> options)
    {
        _settings = options.Value;
        Console.WriteLine($"Using provider: {_settings.Provider}");
        Console.WriteLine($"Using model: {_settings.Model}");
    }
}
```

---

### LLMProvider

**Namespace:** `LLM.Nexus.Settings`

**Description:** Specifies the available LLM providers.

#### Enum Values

| Value | Description |
|-------|-------------|
| `OpenAI` | OpenAI provider (GPT models) |
| `Anthropic` | Anthropic provider (Claude models) |

#### Example

```csharp
var settings = new LLMSettings
{
    Provider = LLMProvider.OpenAI,
    ApiKey = "sk-...",
    Model = "gpt-4"
};
```

---

## Dependency Injection

### DependencyInjection Class

**Namespace:** `LLM.Nexus`

**Description:** Provides extension methods for registering LLM services with dependency injection.

#### Methods

##### AddLLMServices

Adds LLM services to the specified `IServiceCollection`.

```csharp
public static IServiceCollection AddLLMServices(
    this IServiceCollection services
)
```

**Parameters:**
- `services` (`IServiceCollection`): The service collection to add services to.

**Returns:** `IServiceCollection` - The service collection for chaining.

**Registered Services:**

| Service Type | Implementation | Lifetime | Description |
|-------------|----------------|----------|-------------|
| `ILLMServiceFactory` | `LLMServiceFactory` | Singleton | Factory for creating LLM services |
| `IAnthropicService` | `AnthropicService` | Singleton | Anthropic provider implementation |
| `IOpenAIService` | `OpenAIService` | Singleton | OpenAI provider implementation |
| `IOptions<LLMSettings>` | - | Singleton | Configuration options |

**Configuration Binding:**
- Binds `LLMSettings` from the `"LLMSettings"` configuration section
- Validates data annotations on startup
- Throws on invalid configuration

**Example:**

```csharp
using LLM.Nexus;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Register LLM services
builder.Services.AddLLMServices();

var app = builder.Build();
```

**Usage in Services:**

```csharp
public class MyService
{
    private readonly ILLMService _llmService;

    public MyService(ILLMServiceFactory factory)
    {
        // Factory automatically selects the configured provider
        _llmService = factory.CreateService();
    }

    public async Task<string> ProcessAsync(string input)
    {
        var response = await _llmService.GenerateResponseAsync(input);
        return response.Content;
    }
}
```

---

## Extension Methods

### IServiceCollection Extensions

#### AddLLMServices

**Namespace:** `LLM.Nexus`

**Signature:**
```csharp
public static IServiceCollection AddLLMServices(
    this IServiceCollection services
)
```

**Description:** Registers all LLM services with the dependency injection container.

**Performs:**
1. Binds `LLMSettings` from configuration
2. Registers validation for settings
3. Registers provider implementations as singletons
4. Registers factory as singleton
5. Configures HTTP clients with proper lifetimes

**Example:**
```csharp
builder.Services.AddLLMServices();
```

---

## Type Reference

### Quick Reference Table

| Type | Kind | Namespace | Access |
|------|------|-----------|--------|
| `ILLMService` | Interface | `LLM.Nexus` | Public |
| `ILLMServiceFactory` | Interface | `LLM.Nexus` | Public |
| `LLMRequest` | Class | `LLM.Nexus.Models` | Public |
| `LLMResponse` | Class | `LLM.Nexus.Models` | Public |
| `UsageInfo` | Class | `LLM.Nexus.Models` | Public |
| `LLMSettings` | Class | `LLM.Nexus.Settings` | Public |
| `LLMProvider` | Enum | `LLM.Nexus.Settings` | Public |
| `DependencyInjection` | Static Class | `LLM.Nexus` | Public |

### Type Hierarchy

```
LLM.Nexus
├── ILLMService
├── ILLMServiceFactory
└── DependencyInjection (static)

LLM.Nexus.Models
├── LLMRequest
├── LLMResponse
└── UsageInfo

LLM.Nexus.Settings
├── LLMSettings
└── LLMProvider (enum)
```

---

## Complete API Example

Here's a comprehensive example showing all major APIs in use:

```csharp
using LLM.Nexus;
using LLM.Nexus.Models;
using LLM.Nexus.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// 1. Setup and Configuration
var builder = Host.CreateApplicationBuilder(args);

// Configure settings in appsettings.json
/*
{
  "LLMSettings": {
    "Provider": "OpenAI",
    "ApiKey": "sk-...",
    "Model": "gpt-4",
    "MaxTokens": 2000
  }
}
*/

// 2. Register Services
builder.Services.AddLLMServices();
builder.Services.AddTransient<AIAssistant>();

var app = builder.Build();

// 3. Use the Services
var assistant = app.Services.GetRequiredService<AIAssistant>();

// Simple usage
var simpleResponse = await assistant.AskSimpleQuestionAsync("What is AI?");
Console.WriteLine($"Simple: {simpleResponse}");

// Advanced usage with full request
var advancedResponse = await assistant.AskAdvancedQuestionAsync(
    "Write a haiku about programming",
    systemMessage: "You are a creative poet",
    temperature: 0.9
);

Console.WriteLine($"Advanced: {advancedResponse.Content}");
Console.WriteLine($"Tokens: {advancedResponse.Usage.TotalTokens}");
Console.WriteLine($"Model: {advancedResponse.Model}");

// Service Implementation
public class AIAssistant
{
    private readonly ILLMService _llmService;

    public AIAssistant(ILLMServiceFactory factory)
    {
        _llmService = factory.CreateService();
    }

    public async Task<string> AskSimpleQuestionAsync(string question)
    {
        var response = await _llmService.GenerateResponseAsync(question);
        return response.Content;
    }

    public async Task<LLMResponse> AskAdvancedQuestionAsync(
        string question,
        string systemMessage = null,
        double? temperature = null)
    {
        var request = new LLMRequest
        {
            Prompt = question,
            SystemMessage = systemMessage,
            Temperature = temperature,
            MaxTokens = 500
        };

        return await _llmService.GenerateResponseAsync(request);
    }
}
```

---

## Version History

### Version 1.0.0

**Initial Release**

- Core interfaces: `ILLMService`, `ILLMServiceFactory`
- Request/Response models with validation
- OpenAI and Anthropic provider support
- Dependency injection integration
- Configuration with data annotations
- Comprehensive error handling
- 100% test coverage

---

## Support

For issues, questions, or contributions, please visit:
- **GitHub Repository**: https://github.com/yourusername/llm.nexus
- **NuGet Package**: https://www.nuget.org/packages/LLM.Nexus
- **Documentation**: See [README.md](README.md)

---

**Copyright (c) 2025 Alexandros Mouratidis**
**License:** MIT
