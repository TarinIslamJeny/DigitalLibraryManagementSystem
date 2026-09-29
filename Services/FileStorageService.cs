using Microsoft.AspNetCore.Hosting;

namespace DigitalLibraryManagementSystem.Services;

public class FileStorageService
{
    public const long MaxCoverSize = 5 * 1024 * 1024;
    public const long MaxDigitalBookSize = 50 * 1024 * 1024;

    private readonly IWebHostEnvironment _environment;

    public FileStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    private string WebRootPath => _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");

    public void EnsureDirectories()
    {
        Directory.CreateDirectory(Path.Combine(WebRootPath, "images"));
        Directory.CreateDirectory(Path.Combine(WebRootPath, "uploads", "covers"));
        Directory.CreateDirectory(Path.Combine(WebRootPath, "uploads", "books"));
    }

    public string GetCoverImagePath(string? relativePath)
    {
        return TryResolveWebPath(relativePath, out _) ? NormalizeWebPath(relativePath!) : "/images/default-cover.jpg";
    }

    public bool DigitalFileExists(string? relativePath)
    {
        return TryResolveOwnedPath(relativePath, "books", out _);
    }

    public async Task<string?> SaveCoverImageAsync(IFormFile? file)
    {
        return await SaveUploadedFileAsync(file, "covers", [".jpg", ".jpeg", ".png", ".webp"], MaxCoverSize, "image");
    }

    public async Task<string?> SaveDigitalBookAsync(IFormFile? file)
    {
        return await SaveUploadedFileAsync(file, "books", [".pdf"], MaxDigitalBookSize, "pdf");
    }

    public async Task<string?> SaveUploadedFileAsync(IFormFile? file, string folderName, string[] allowedExtensions, long maxFileSizeInBytes, string contentKind)
    {
        if (file == null || file.Length == 0)
        {
            return null;
        }

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Only these file types are allowed: {string.Join(", ", allowedExtensions)}.");
        }

        if (file.Length > maxFileSizeInBytes)
        {
            throw new InvalidOperationException($"The file exceeds the {maxFileSizeInBytes / (1024 * 1024)} MB size limit.");
        }

        await ValidateContentAsync(file, contentKind, extension);
        EnsureDirectories();

        var safeExtension = extension.ToLowerInvariant();
        var fileName = $"{Guid.NewGuid():N}{safeExtension}";
        var destination = Path.Combine(WebRootPath, "uploads", folderName, fileName);
        await using (var output = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
            await file.CopyToAsync(output);
        }

        return $"/uploads/{folderName}/{fileName}";
    }

    public void DeleteCoverIfOwned(string? relativePath) => DeleteOwnedFileIfExists(relativePath, "covers");

    public void DeleteDigitalBookIfOwned(string? relativePath) => DeleteOwnedFileIfExists(relativePath, "books");

    private async Task ValidateContentAsync(IFormFile file, string contentKind, string extension)
    {
        var contentType = file.ContentType?.Trim();
        if (!string.IsNullOrEmpty(contentType) && !string.Equals(contentType, "application/octet-stream", StringComparison.OrdinalIgnoreCase))
        {
            var allowedContentTypes = contentKind == "pdf"
                ? new[] { "application/pdf" }
                : extension.ToLowerInvariant() switch
                {
                    ".jpg" or ".jpeg" => new[] { "image/jpeg", "image/pjpeg" },
                    ".png" => new[] { "image/png" },
                    ".webp" => new[] { "image/webp" },
                    _ => Array.Empty<string>()
                };

            if (!allowedContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("The uploaded file content type does not match its extension.");
            }
        }

        await using var stream = file.OpenReadStream();
        var header = new byte[contentKind == "pdf" ? 1024 : 12];
        var bytesRead = await stream.ReadAsync(header);
        var isValid = contentKind == "pdf"
            ? ContainsPdfHeader(header.AsSpan(0, bytesRead))
            : IsValidImageHeader(header.AsSpan(0, bytesRead), extension);

        if (!isValid)
        {
            throw new InvalidOperationException(contentKind == "pdf"
                ? "The uploaded file is not a valid PDF."
                : "The uploaded file is not a valid image for the selected extension.");
        }
    }

    private static bool IsValidImageHeader(ReadOnlySpan<byte> header, string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => header.Length >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            ".png" => header.Length >= 8 && header[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
            ".webp" => header.Length >= 12 && header[..4].SequenceEqual("RIFF"u8) && header[8..12].SequenceEqual("WEBP"u8),
            _ => false
        };
    }

    private static bool ContainsPdfHeader(ReadOnlySpan<byte> header)
    {
        for (var index = 0; index <= header.Length - 5; index++)
        {
            if (header.Slice(index, 5).SequenceEqual("%PDF-"u8))
            {
                return true;
            }
        }

        return false;
    }

    private void DeleteOwnedFileIfExists(string? relativePath, string folderName)
    {
        if (TryResolveOwnedPath(relativePath, folderName, out var physicalPath))
        {
            File.Delete(physicalPath);
        }
    }

    private bool TryResolveOwnedPath(string? relativePath, string folderName, out string physicalPath)
    {
        physicalPath = string.Empty;
        var normalized = NormalizeWebPath(relativePath ?? string.Empty);
        if (!normalized.StartsWith($"/uploads/{folderName}/", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var fileName = normalized[$"/uploads/{folderName}/".Length..];
        if (string.IsNullOrWhiteSpace(fileName) || fileName.Contains('/') || fileName is "." or ".." || fileName.Contains(':'))
        {
            return false;
        }

        var folderPath = Path.GetFullPath(Path.Combine(WebRootPath, "uploads", folderName));
        physicalPath = Path.GetFullPath(Path.Combine(folderPath, fileName));
        return physicalPath.StartsWith(folderPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            && File.Exists(physicalPath);
    }

    private bool TryResolveWebPath(string? relativePath, out string physicalPath)
    {
        physicalPath = string.Empty;
        if (string.IsNullOrWhiteSpace(relativePath) || relativePath.Contains('\\') || relativePath.Contains('?') || relativePath.Contains('#'))
        {
            return false;
        }

        var normalized = NormalizeWebPath(relativePath);
        if (normalized.Length == 0 || normalized[0] != '/' || normalized.StartsWith("//", StringComparison.Ordinal))
        {
            return false;
        }

        var root = Path.GetFullPath(WebRootPath);
        physicalPath = Path.GetFullPath(Path.Combine(root, normalized.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)));
        return physicalPath.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            && File.Exists(physicalPath);
    }

    private static string NormalizeWebPath(string relativePath) => relativePath.Replace('\\', '/');
}
