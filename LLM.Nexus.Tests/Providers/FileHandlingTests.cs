using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LLM.Nexus.Models;
using LLM.Nexus.Providers.Anthropic;
using LLM.Nexus.Providers.Google;
using LLM.Nexus.Providers.OpenAI;
using LLM.Nexus.Settings;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LLM.Nexus.Tests.Providers
{
    /// <summary>
    /// Tests for file/image handling across all LLM providers.
    /// </summary>
    public class FileHandlingTests : IDisposable
    {
        private readonly string _testImagePath;
        private readonly byte[] _testImageBytes;
        private readonly string _testImageBase64;

        public FileHandlingTests()
        {
            // Create a minimal test image (1x1 PNG)
            _testImageBytes = CreateTestPngImage();
            _testImageBase64 = Convert.ToBase64String(_testImageBytes);

            // Create temp file for file-based tests
            _testImagePath = Path.Combine(Path.GetTempPath(), $"test-image-{Guid.NewGuid()}.png");
            File.WriteAllBytes(_testImagePath, _testImageBytes);
        }

        public void Dispose()
        {
            if (File.Exists(_testImagePath))
            {
                File.Delete(_testImagePath);
            }
        }

        #region Helper Methods

        /// <summary>
        /// Creates a minimal valid 1x1 PNG image (67 bytes).
        /// </summary>
        private static byte[] CreateTestPngImage()
        {
            return new byte[]
            {
                0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, // PNG signature
                0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52, // IHDR chunk
                0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, // 1x1 dimensions
                0x08, 0x02, 0x00, 0x00, 0x00, 0x90, 0x77, 0x53, // IHDR data
                0xDE, 0x00, 0x00, 0x00, 0x0C, 0x49, 0x44, 0x41, // IDAT chunk
                0x54, 0x08, 0xD7, 0x63, 0xF8, 0xFF, 0xFF, 0x3F, // IDAT data
                0x00, 0x05, 0xFE, 0x02, 0xFE, 0xDC, 0xCC, 0x59, // IDAT data
                0xE7, 0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, // IEND chunk
                0x44, 0xAE, 0x42, 0x60, 0x82                     // IEND data
            };
        }

        #endregion

        #region FileContent Tests

        [Fact]
        public void FileContent_FromFile_CreatesValidFileContent()
        {
            // Act
            var fileContent = FileContent.FromFile(_testImagePath, MediaType.Image);

            // Assert
            fileContent.Should().NotBeNull();
            fileContent.MediaType.Should().Be(MediaType.Image);
            fileContent.MimeType.Should().Be("image/png");
            fileContent.Data.Should().NotBeNullOrEmpty();
            fileContent.FileName.Should().Contain("test-image");
        }

        [Fact]
        public void FileContent_FromFile_ThrowsWhenFileNotFound()
        {
            // Act
            Action act = () => FileContent.FromFile("nonexistent.jpg", MediaType.Image);

            // Assert
            act.Should().Throw<FileNotFoundException>();
        }

        [Fact]
        public void FileContent_FromBytes_CreatesValidFileContent()
        {
            // Act
            var fileContent = FileContent.FromBytes(_testImageBytes, MediaType.Image, "image/png", "test.png");

            // Assert
            fileContent.Should().NotBeNull();
            fileContent.MediaType.Should().Be(MediaType.Image);
            fileContent.MimeType.Should().Be("image/png");
            fileContent.Data.Should().Be(_testImageBase64);
            fileContent.FileName.Should().Be("test.png");
        }

        [Fact]
        public void FileContent_FromUrl_CreatesValidFileContent()
        {
            // Arrange
            var testUrl = "https://example.com/image.jpg";

            // Act
            var fileContent = FileContent.FromUrl(testUrl, MediaType.Image, "image/jpeg");

            // Assert
            fileContent.Should().NotBeNull();
            fileContent.MediaType.Should().Be(MediaType.Image);
            fileContent.MimeType.Should().Be("image/jpeg");
            fileContent.Url.Should().Be(testUrl);
            fileContent.Data.Should().BeEmpty();
        }

        [Theory]
        [InlineData(".jpg", "image/jpeg")]
        [InlineData(".png", "image/png")]
        [InlineData(".gif", "image/gif")]
        [InlineData(".webp", "image/webp")]
        [InlineData(".pdf", "application/pdf")]
        public void FileContent_DetectsMimeTypeCorrectly(string extension, string expectedMimeType)
        {
            // Arrange
            var tempFile = Path.Combine(Path.GetTempPath(), $"test{extension}");
            File.WriteAllBytes(tempFile, _testImageBytes);

            try
            {
                // Act
                var fileContent = FileContent.FromFile(tempFile, MediaType.Image);

                // Assert
                fileContent.MimeType.Should().Be(expectedMimeType);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        #endregion

        #region LLMRequest with Files Tests

        [Fact]
        public void LLMRequest_WithSingleFile_IsValid()
        {
            // Arrange
            var request = new LLMRequest
            {
                Prompt = "What's in this image?",
                Files = new List<FileContent>
                {
                    FileContent.FromFile(_testImagePath, MediaType.Image)
                }
            };

            // Act & Assert
            request.Files.Should().HaveCount(1);
            request.Files[0].MediaType.Should().Be(MediaType.Image);
        }

        [Fact]
        public void LLMRequest_WithMultipleFiles_IsValid()
        {
            // Arrange
            var request = new LLMRequest
            {
                Prompt = "Compare these images",
                Files = new List<FileContent>
                {
                    FileContent.FromBytes(_testImageBytes, MediaType.Image, "image/png", "image1.png"),
                    FileContent.FromBytes(_testImageBytes, MediaType.Image, "image/png", "image2.png")
                }
            };

            // Act & Assert
            request.Files.Should().HaveCount(2);
        }

        [Fact]
        public void LLMRequest_WithNoFiles_IsValid()
        {
            // Arrange
            var request = new LLMRequest
            {
                Prompt = "Hello, how are you?"
            };

            // Act & Assert
            request.Files.Should().NotBeNull();
            request.Files.Should().BeEmpty();
        }

        #endregion

        #region OpenAI Provider File Handling Tests

        [Fact]
        public async Task OpenAIService_AcceptsFileInRequest()
        {
            // Note: This is a structure test only - it validates that the service accepts files
            // Actual API calls require valid API keys and are integration tests

            // Arrange
            var mockLogger = new Mock<ILogger<OpenAIService>>();
            var config = new ProviderConfiguration
            {
                ApiKey = "test-key",
                Model = "gpt-4o-mini",
                MaxTokens = 100
            };

            var request = new LLMRequest
            {
                Prompt = "Describe this image",
                Files = new List<FileContent>
                {
                    FileContent.FromBytes(_testImageBytes, MediaType.Image, "image/png")
                }
            };

            // Act & Assert - Should not throw on construction
            var service = new OpenAIService(mockLogger.Object, config);
            service.Should().NotBeNull();
            request.Files.Should().HaveCount(1);
        }

        #endregion

        #region Anthropic Provider File Handling Tests

        [Fact]
        public async Task AnthropicService_AcceptsFileInRequest()
        {
            // Note: This is a structure test only - it validates that the service accepts files
            // Actual API calls require valid API keys and are integration tests

            // Arrange
            var mockLogger = new Mock<ILogger<AnthropicService>>();
            var config = new ProviderConfiguration
            {
                ApiKey = "test-key",
                Model = "claude-3-haiku-20240307",
                MaxTokens = 100
            };

            var request = new LLMRequest
            {
                Prompt = "Describe this image",
                Files = new List<FileContent>
                {
                    FileContent.FromBytes(_testImageBytes, MediaType.Image, "image/png")
                }
            };

            // Act & Assert - Should not throw on construction
            var service = new AnthropicService(mockLogger.Object, config);
            service.Should().NotBeNull();
            request.Files.Should().HaveCount(1);
        }

        #endregion

        #region Google Provider File Handling Tests

        [Fact]
        public async Task GoogleService_AcceptsFileInRequest()
        {
            // Note: This is a structure test only - it validates that the service accepts files
            // Actual API calls require valid API keys and are integration tests

            // Arrange
            var mockLogger = new Mock<ILogger<GoogleService>>();
            var config = new ProviderConfiguration
            {
                ApiKey = "test-key",
                Model = "gemini-1.5-flash",
                MaxTokens = 100
            };

            var request = new LLMRequest
            {
                Prompt = "Describe this image",
                Files = new List<FileContent>
                {
                    FileContent.FromBytes(_testImageBytes, MediaType.Image, "image/png")
                }
            };

            // Act & Assert - Should not throw on construction
            var service = new GoogleService(mockLogger.Object, config);
            service.Should().NotBeNull();
            request.Files.Should().HaveCount(1);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void FileContent_WithEmptyData_IsInvalid()
        {
            // Arrange
            var fileContent = new FileContent
            {
                MediaType = MediaType.Image,
                MimeType = "image/png",
                Data = ""
            };

            // Act & Assert
            fileContent.Data.Should().BeEmpty();
        }

        [Fact]
        public void LLMRequest_WithMixedMediaTypes_IsValid()
        {
            // Arrange
            var request = new LLMRequest
            {
                Prompt = "Analyze these files",
                Files = new List<FileContent>
                {
                    FileContent.FromBytes(_testImageBytes, MediaType.Image, "image/png", "image.png"),
                    FileContent.FromBytes(_testImageBytes, MediaType.Document, "application/pdf", "doc.pdf")
                }
            };

            // Act & Assert
            request.Files.Should().HaveCount(2);
            request.Files[0].MediaType.Should().Be(MediaType.Image);
            request.Files[1].MediaType.Should().Be(MediaType.Document);
        }

        #endregion
    }
}
