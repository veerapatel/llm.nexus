# LLM.Nexus Public API Reference

**Version:** 2.1.0
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
  - [FileContent](#filecontent)
  - [MediaType](#mediatype)
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

**Description:** Factory for creating LLM service instances based on configuration. Supports multiple named provider configurations in the same application.

#### Methods

##### CreateService()

Creates an LLM service instance for the default provider. Uses the configured DefaultProvider or the first configured provider if not specified.

```csharp
ILLMService CreateService()
```

**Returns:** `ILLMService` - An instance of `ILLMService` for the default provider.

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

##### CreateService(string providerName)

Creates an LLM service instance for a specific named provider.

```csharp
ILLMService CreateService(string providerName)
```

**Parameters:**
- `providerName` (`string`): The name of the provider configuration to use.

**Returns:** `ILLMService` - An instance of `ILLMService` for the specified provider.

**Exceptions:**
- `ArgumentException`: Thrown when the provider name is null, empty, or not found in configuration.

**Example:**
```csharp
public class MultiProviderService
{
    private readonly ILLMService _openAI;
    private readonly ILLMService _anthropic;

    public MultiProviderService(ILLMServiceFactory factory)
    {
        _openAI = factory.CreateService("openai-gpt4");
        _anthropic = factory.CreateService("anthropic-claude");
    }

    public async Task<string> CompareResponses(string prompt)
    {
        var openAIResponse = await _openAI.GenerateResponseAsync(prompt);
        var anthropicResponse = await _anthropic.GenerateResponseAsync(prompt);
        return $"OpenAI: {openAIResponse.Content}\n\nAnthropic: {anthropicResponse.Content}";
    }
}
```

##### GetConfiguredProviders()

Gets all configured provider names.

```csharp
IEnumerable<string> GetConfiguredProviders()
```

**Returns:** `IEnumerable<string>` - A collection of configured provider names.

**Example:**
```csharp
var factory = serviceProvider.GetRequiredService<ILLMServiceFactory>();
var providers = factory.GetConfiguredProviders();
Console.WriteLine($"Available providers: {string.Join(", ", providers)}");
```

##### GetDefaultProviderName()

Gets the name of the default provider.

```csharp
string GetDefaultProviderName()
```

**Returns:** `string` - The name of the default provider, or the first configured provider if no default is set.

**Example:**
```csharp
var factory = serviceProvider.GetRequiredService<ILLMServiceFactory>();
var defaultProvider = factory.GetDefaultProviderName();
Console.WriteLine($"Default provider: {defaultProvider}");
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
| `Files` | `List<FileContent>` | No | - | Empty list | List of files/media to include with the request |
| `AdditionalParameters` | `Dictionary<string, object>` | No | - | `null` | Provider-specific parameters |

#### Validation Rules

- **Prompt**: Required, must be between 1 and 1,000,000 characters
- **MaxTokens**: If specified, must be between 1 and 1,000,000
- **Temperature**: If specified, must be between 0.0 and 2.0

#### Examples

**Text-Only Request:**
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

**Multimodal Request with Image:**
```csharp
var request = new LLMRequest
{
    Prompt = "What's in this image? Describe it in detail.",
    MaxTokens = 500,
    Files = new List<FileContent>
    {
        FileContent.FromFile("photo.jpg", MediaType.Image)
    }
};
```

**Multimodal Request with Multiple Files:**
```csharp
var request = new LLMRequest
{
    Prompt = "Compare these two images and identify the differences.",
    MaxTokens = 1000,
    Files = new List<FileContent>
    {
        FileContent.FromFile("image1.jpg", MediaType.Image),
        FileContent.FromFile("image2.jpg", MediaType.Image)
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

### FileContent

**Namespace:** `LLM.Nexus.Models`

**Description:** Represents a file or media content to be sent to an LLM for multimodal requests (vision, document analysis, etc.).

#### Properties

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `MediaType` | `MediaType` | Yes | The type of media (Image, Document, Audio, Video) |
| `MimeType` | `string` | Yes | MIME type of the file (e.g., "image/jpeg", "application/pdf") |
| `Data` | `string` | Conditional | Base64-encoded file data (required if Url is not provided) |
| `FileName` | `string` | No | Optional file name for reference |
| `Url` | `string` | Conditional | URL to remote file (OpenAI only, required if Data is not provided) |

#### Static Factory Methods

##### FromFile

Creates a `FileContent` from a file path. Automatically detects MIME type based on file extension.

```csharp
public static FileContent FromFile(string filePath, MediaType mediaType)
```

**Parameters:**
- `filePath` (`string`): The path to the file to load.
- `mediaType` (`MediaType`): The type of media (Image, Document, Audio, Video).

**Returns:** `FileContent` - A new FileContent instance with base64-encoded data.

**Exceptions:**
- `FileNotFoundException`: When the specified file does not exist.

**Example:**
```csharp
var imageFile = FileContent.FromFile("photo.jpg", MediaType.Image);
var pdfFile = FileContent.FromFile("document.pdf", MediaType.Document);
```

##### FromBytes

Creates a `FileContent` from a byte array.

```csharp
public static FileContent FromBytes(
    byte[] data,
    MediaType mediaType,
    string mimeType,
    string fileName = null
)
```

**Parameters:**
- `data` (`byte[]`): The file data as bytes.
- `mediaType` (`MediaType`): The type of media.
- `mimeType` (`string`): The MIME type (e.g., "image/jpeg").
- `fileName` (`string`, optional): Optional file name for reference.

**Returns:** `FileContent` - A new FileContent instance with base64-encoded data.

**Example:**
```csharp
byte[] imageBytes = await File.ReadAllBytesAsync("upload.jpg");
var fileContent = FileContent.FromBytes(imageBytes, MediaType.Image, "image/jpeg", "upload.jpg");
```

##### FromUrl

Creates a `FileContent` from a URL (all providers supported).

```csharp
public static FileContent FromUrl(
    string url,
    MediaType mediaType,
    string mimeType
)
```

**Parameters:**
- `url` (`string`): The URL of the remote file.
- `mediaType` (`MediaType`): The type of media.
- `mimeType` (`string`): The MIME type of the file.

**Returns:** `FileContent` - A new FileContent instance with URL reference.

**Example:**
```csharp
var fileContent = FileContent.FromUrl(
    "https://example.com/image.jpg",
    MediaType.Image,
    "image/jpeg"
);
```

**Provider-Specific Behavior:**
- **OpenAI**: URLs are passed directly to the API (native support)
- **Anthropic**: URLs are automatically downloaded and converted to base64
- **Google**: URLs are automatically downloaded and converted to base64

#### Supported MIME Types

**Images:**
- `image/jpeg` - JPEG images (.jpg, .jpeg)
- `image/png` - PNG images (.png)
- `image/gif` - GIF images (.gif)
- `image/webp` - WebP images (.webp)
- `image/bmp` - Bitmap images (.bmp)
- `image/svg+xml` - SVG images (.svg)

**Documents:**
- `application/pdf` - PDF documents (.pdf)
- `text/plain` - Plain text (.txt)
- `text/csv` - CSV files (.csv)
- `application/json` - JSON files (.json)
- `application/xml` - XML files (.xml)
- `application/msword` - Word documents (.doc)
- `application/vnd.openxmlformats-officedocument.wordprocessingml.document` - Word documents (.docx)

**Audio:**
- `audio/mpeg` - MP3 audio (.mp3)
- `audio/wav` - WAV audio (.wav)
- `audio/ogg` - OGG audio (.ogg)

**Video:**
- `video/mp4` - MP4 video (.mp4)
- `video/x-msvideo` - AVI video (.avi)
- `video/quicktime` - QuickTime video (.mov)

#### Provider Support

| Provider | Image Support | Document Support | URL Support | Notes |
|----------|---------------|------------------|-------------|-------|
| **OpenAI** | ✅ | Limited | ✅ Native | Supports image URLs and base64 |
| **Anthropic** | ✅ | ✅ | ✅ Auto-download | URLs automatically downloaded and converted to base64 |
| **Google** | ✅ | ✅ | ✅ Auto-download | URLs automatically downloaded and converted to base64 |

#### Complete Example

```csharp
// Analyze an image from file
var request = new LLMRequest
{
    Prompt = "What's in this image?",
    Files = new List<FileContent>
    {
        FileContent.FromFile("vacation-photo.jpg", MediaType.Image)
    }
};

var response = await llmService.GenerateResponseAsync(request);
Console.WriteLine(response.Content);

// Analyze a PDF document
var docRequest = new LLMRequest
{
    Prompt = "Summarize this document",
    Files = new List<FileContent>
    {
        FileContent.FromFile("report.pdf", MediaType.Document)
    }
};

var docResponse = await llmService.GenerateResponseAsync(docRequest);
Console.WriteLine(docResponse.Content);

// Compare two images
var compareRequest = new LLMRequest
{
    Prompt = "What are the differences between these images?",
    Files = new List<FileContent>
    {
        FileContent.FromFile("before.jpg", MediaType.Image),
        FileContent.FromFile("after.jpg", MediaType.Image)
    }
};

var compareResponse = await llmService.GenerateResponseAsync(compareRequest);
Console.WriteLine(compareResponse.Content);
```

---

### MediaType

**Namespace:** `LLM.Nexus.Models`

**Description:** Specifies the type of media content in a multimodal request.

#### Enum Values

| Value | Description | Supported Formats |
|-------|-------------|-------------------|
| `Image` | Image content | JPEG, PNG, GIF, WebP, BMP, SVG |
| `Document` | Document content | PDF, TXT, CSV, JSON, XML, DOC, DOCX |
| `Audio` | Audio content | MP3, WAV, OGG |
| `Video` | Video content | MP4, AVI, MOV |

#### Example

```csharp
// Image analysis
var imageFile = FileContent.FromFile("photo.jpg", MediaType.Image);

// Document analysis
var docFile = FileContent.FromFile("report.pdf", MediaType.Document);

// Audio transcription (future support)
var audioFile = FileContent.FromFile("recording.mp3", MediaType.Audio);

// Video analysis (future support)
var videoFile = FileContent.FromFile("clip.mp4", MediaType.Video);
```

---

## Configuration

### LLMSettings

**Namespace:** `LLM.Nexus.Settings`

**Description:** Configuration settings for LLM providers. Supports multiple named provider configurations in the same application.

#### Properties

| Property | Type | Required | Default | Description |
|----------|------|----------|---------|-------------|
| `Providers` | `Dictionary<string, ProviderConfiguration>` | Yes | - | Multiple named provider configurations |
| `DefaultProvider` | `string` | No | - | Name of the default provider (uses first if not set) |

#### Constants

| Constant | Type | Value | Description |
|----------|------|-------|-------------|
| `Section` | `string` | `"LLMSettings"` | Configuration section name |

#### Configuration Example (appsettings.json)

**Single Provider:**
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

**Multiple Providers:**
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
        "ApiKey": "your-google-ai-studio-api-key",
        "Model": "gemini-2.0-flash",
        "MaxTokens": 8000
      }
    }
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
        Console.WriteLine($"Configured providers: {string.Join(", ", _settings.Providers.Keys)}");
        Console.WriteLine($"Default provider: {_settings.DefaultProvider}");
    }
}
```

---

### ProviderConfiguration

**Namespace:** `LLM.Nexus.Settings`

**Description:** Configuration for a specific LLM provider instance.

#### Properties

| Property | Type | Required | Default | Description |
|----------|------|----------|---------|-------------|
| `Provider` | `LLMProvider` | Yes | - | The LLM provider type |
| `ApiKey` | `string` | Yes | - | API key for authentication |
| `Model` | `string` | Yes | - | Model identifier |
| `MaxTokens` | `int?` | No | 2000 | Maximum tokens to generate |
| `Stream` | `bool?` | No | `null` | Enable streaming responses (future) |

#### Example

```csharp
var config = new ProviderConfiguration
{
    Provider = LLMProvider.OpenAI,
    ApiKey = "sk-...",
    Model = "gpt-4",
    MaxTokens = 2000
};
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
| `Google` | Google provider (Gemini models) |

#### Example

```csharp
var config = new ProviderConfiguration
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
| `IOptions<LLMSettings>` | - | Singleton | Configuration options |

**Configuration Binding:**
- Binds `LLMSettings` from the `"LLMSettings"` configuration section
- Validates data annotations on startup
- Throws on invalid configuration
- Supports multiple provider configurations

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
// Single provider usage
public class MyService
{
    private readonly ILLMService _llmService;

    public MyService(ILLMServiceFactory factory)
    {
        // Factory automatically selects the default provider
        _llmService = factory.CreateService();
    }

    public async Task<string> ProcessAsync(string input)
    {
        var response = await _llmService.GenerateResponseAsync(input);
        return response.Content;
    }
}

// Multi-provider usage
public class MultiProviderService
{
    private readonly ILLMService _openAI;
    private readonly ILLMService _anthropic;
    private readonly ILLMService _google;

    public MultiProviderService(ILLMServiceFactory factory)
    {
        _openAI = factory.CreateService("openai-gpt4");
        _anthropic = factory.CreateService("anthropic-claude");
        _google = factory.CreateService("google-gemini");
    }

    public async Task<string> GetBestResponse(string input)
    {
        // Call multiple providers and compare results
        var tasks = new[]
        {
            _openAI.GenerateResponseAsync(input),
            _anthropic.GenerateResponseAsync(input),
            _google.GenerateResponseAsync(input)
        };

        var responses = await Task.WhenAll(tasks);
        // ... logic to select best response
        return responses[0].Content;
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
| `FileContent` | Class | `LLM.Nexus.Models` | Public |
| `MediaType` | Enum | `LLM.Nexus.Models` | Public |
| `LLMSettings` | Class | `LLM.Nexus.Settings` | Public |
| `ProviderConfiguration` | Class | `LLM.Nexus.Settings` | Public |
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
├── UsageInfo
├── FileContent
└── MediaType (enum)

LLM.Nexus.Settings
├── LLMSettings
├── ProviderConfiguration
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
        "ApiKey": "your-google-ai-studio-api-key",
        "Model": "gemini-2.0-flash",
        "MaxTokens": 8000
      }
    }
  }
}
*/

// 2. Register Services
builder.Services.AddLLMServices();
builder.Services.AddTransient<AIAssistant>();

var app = builder.Build();

// 3. Use the Services
var assistant = app.Services.GetRequiredService<AIAssistant>();

// Simple usage with default provider
var simpleResponse = await assistant.AskSimpleQuestionAsync("What is AI?");
Console.WriteLine($"Simple: {simpleResponse}");

// Advanced usage with specific provider
var advancedResponse = await assistant.AskAdvancedQuestionAsync(
    "Write a haiku about programming",
    providerName: "anthropic-claude",
    systemMessage: "You are a creative poet",
    temperature: 0.9
);

Console.WriteLine($"Advanced: {advancedResponse.Content}");
Console.WriteLine($"Tokens: {advancedResponse.Usage.TotalTokens}");
Console.WriteLine($"Model: {advancedResponse.Model}");
Console.WriteLine($"Provider: {advancedResponse.Provider}");

// Multi-provider comparison
var comparison = await assistant.CompareProvidersAsync("Explain quantum computing");
Console.WriteLine(comparison);

// Service Implementation
public class AIAssistant
{
    private readonly ILLMServiceFactory _factory;

    public AIAssistant(ILLMServiceFactory factory)
    {
        _factory = factory;
    }

    public async Task<string> AskSimpleQuestionAsync(string question)
    {
        // Uses default provider
        var service = _factory.CreateService();
        var response = await service.GenerateResponseAsync(question);
        return response.Content;
    }

    public async Task<LLMResponse> AskAdvancedQuestionAsync(
        string question,
        string providerName = null,
        string systemMessage = null,
        double? temperature = null)
    {
        // Use specific provider or default
        var service = string.IsNullOrEmpty(providerName)
            ? _factory.CreateService()
            : _factory.CreateService(providerName);

        var request = new LLMRequest
        {
            Prompt = question,
            SystemMessage = systemMessage,
            Temperature = temperature,
            MaxTokens = 500
        };

        return await service.GenerateResponseAsync(request);
    }

    public async Task<string> CompareProvidersAsync(string question)
    {
        var providers = _factory.GetConfiguredProviders();
        var results = new List<string>();

        foreach (var providerName in providers)
        {
            var service = _factory.CreateService(providerName);
            var response = await service.GenerateResponseAsync(question);
            results.Add($"{response.Provider}: {response.Content}");
        }

        return string.Join("\n\n---\n\n", results);
    }
}
```

---

## Version History

### Version 2.1.0

**Multimodal Support & Google AI SDK Migration**

- **NEW**: Multimodal support - Send images, documents, and other files to LLMs
- **NEW**: `FileContent` class for handling files with helper methods (`FromFile`, `FromBytes`, `FromUrl`)
- **NEW**: `MediaType` enum for specifying file types (Image, Document, Audio, Video)
- **NEW**: `Files` property on `LLMRequest` for multimodal requests
- **NEW**: Support for vision-capable models across all providers
- **BREAKING CHANGE**: Migrated Google provider from `Google.Cloud.AIPlatform.V1` to `Google_GenerativeAI` SDK
- Simplified Google authentication - now uses API key directly (Google AI Studio)
- Improved Google provider implementation with cleaner API and multimodal support
- Better support for system instructions in Google Gemini models
- All existing functionality maintained (temperature, max tokens, system messages)
- 82 tests passing (added 17 file handling tests)

**New Features:**

Multimodal requests with images:
```csharp
var request = new LLMRequest
{
    Prompt = "What's in this image?",
    Files = new List<FileContent>
    {
        FileContent.FromFile("photo.jpg", MediaType.Image)
    }
};
```

**Migration Guide:**

If using the Google provider, you must:
1. Obtain a Google AI Studio API key from https://makersuite.google.com/app/apikey
2. Update your configuration to use the new API key (instead of Vertex AI service account credentials)
3. Model names can optionally include or exclude the "models/" prefix (e.g., both "gemini-2.0-flash" and "models/gemini-2.0-flash" work)

### Version 2.0.0

**Major Release - Multi-Provider Support**

- **BREAKING CHANGE**: Configuration schema updated to support multiple providers
- Multi-provider architecture allows simultaneous use of multiple LLM providers
- Enhanced `ILLMServiceFactory` with provider selection methods
- New `ProviderConfiguration` class for individual provider settings
- Support for default provider specification
- Google (Gemini) provider support added
- Updated documentation and examples
- Simplified dependency injection registration

**Migration Guide:**

Old configuration (v1.x):
```json
{
  "LLMSettings": {
    "Provider": "OpenAI",
    "ApiKey": "sk-...",
    "Model": "gpt-4"
  }
}
```

New configuration (v2.0):
```json
{
  "LLMSettings": {
    "Providers": {
      "default": {
        "Provider": "OpenAI",
        "ApiKey": "sk-...",
        "Model": "gpt-4"
      }
    }
  }
}
```

---

## Support

For issues, questions, or contributions, please visit:
- **GitHub Repository**: https://github.com/yourusername/llm.nexus
- **NuGet Package**: https://www.nuget.org/packages/LLM.Nexus
- **Documentation**: See [README.md](README.md)

---

**Copyright (c) 2025 Alexandros Mouratidis**
**License:** MIT
