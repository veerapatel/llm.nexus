using System;
using System.ComponentModel.DataAnnotations;

namespace LLM.Nexus.Models
{
    /// <summary>
    /// Represents the type of media content.
    /// </summary>
    public enum MediaType
    {
        /// <summary>
        /// Image content (JPEG, PNG, GIF, WebP).
        /// </summary>
        Image,

        /// <summary>
        /// Document content (PDF, text files, etc.).
        /// </summary>
        Document,

        /// <summary>
        /// Audio content.
        /// </summary>
        Audio,

        /// <summary>
        /// Video content.
        /// </summary>
        Video
    }

    /// <summary>
    /// Represents a file or media content to be sent to an LLM.
    /// </summary>
    public class FileContent
    {
        /// <summary>
        /// Gets or sets the type of media.
        /// </summary>
        [Required]
        public MediaType MediaType { get; set; }

        /// <summary>
        /// Gets or sets the MIME type of the file (e.g., "image/jpeg", "application/pdf").
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string MimeType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the base64-encoded file data.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string Data { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the optional file name.
        /// </summary>
        public string? FileName { get; set; }

        /// <summary>
        /// Gets or sets the optional URL if the file is hosted remotely.
        /// Some providers support URLs instead of base64 data.
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// Creates a FileContent from a file path.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <param name="mediaType">The type of media.</param>
        /// <returns>A FileContent instance.</returns>
        public static FileContent FromFile(string filePath, MediaType mediaType)
        {
            if (!System.IO.File.Exists(filePath))
            {
                throw new System.IO.FileNotFoundException($"File not found: {filePath}");
            }

            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            string base64Data = Convert.ToBase64String(fileBytes);
            string mimeType = GetMimeType(filePath);

            return new FileContent
            {
                MediaType = mediaType,
                MimeType = mimeType,
                Data = base64Data,
                FileName = System.IO.Path.GetFileName(filePath)
            };
        }

        /// <summary>
        /// Creates a FileContent from a URL.
        /// </summary>
        /// <param name="url">The URL of the remote file.</param>
        /// <param name="mediaType">The type of media.</param>
        /// <param name="mimeType">The MIME type of the file.</param>
        /// <returns>A FileContent instance.</returns>
        public static FileContent FromUrl(string url, MediaType mediaType, string mimeType)
        {
            return new FileContent
            {
                MediaType = mediaType,
                MimeType = mimeType,
                Url = url,
                Data = string.Empty
            };
        }

        /// <summary>
        /// Creates a FileContent from byte array.
        /// </summary>
        /// <param name="data">The file data as bytes.</param>
        /// <param name="mediaType">The type of media.</param>
        /// <param name="mimeType">The MIME type.</param>
        /// <param name="fileName">Optional file name.</param>
        /// <returns>A FileContent instance.</returns>
        public static FileContent FromBytes(byte[] data, MediaType mediaType, string mimeType, string? fileName = null)
        {
            return new FileContent
            {
                MediaType = mediaType,
                MimeType = mimeType,
                Data = Convert.ToBase64String(data),
                FileName = fileName
            };
        }

        /// <summary>
        /// Gets the MIME type based on file extension.
        /// </summary>
        private static string GetMimeType(string filePath)
        {
            string extension = System.IO.Path.GetExtension(filePath).ToLowerInvariant();

            return extension switch
            {
                // Images
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".bmp" => "image/bmp",
                ".svg" => "image/svg+xml",

                // Documents
                ".pdf" => "application/pdf",
                ".txt" => "text/plain",
                ".csv" => "text/csv",
                ".json" => "application/json",
                ".xml" => "application/xml",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",

                // Audio
                ".mp3" => "audio/mpeg",
                ".wav" => "audio/wav",
                ".ogg" => "audio/ogg",

                // Video
                ".mp4" => "video/mp4",
                ".avi" => "video/x-msvideo",
                ".mov" => "video/quicktime",

                _ => "application/octet-stream"
            };
        }
    }
}
