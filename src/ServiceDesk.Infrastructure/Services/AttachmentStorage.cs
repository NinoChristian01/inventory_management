using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ServiceDesk.Application.Common;
using ServiceDesk.Application.Interfaces;

namespace ServiceDesk.Infrastructure.Services;

public sealed class AttachmentStorage(IOptions<AppOptions> options, IHostEnvironment environment) : IAttachmentStorage
{
    private string RootPath
    {
        get
        {
            var root = options.Value.UploadRoot;
            if (string.IsNullOrWhiteSpace(root)) root = "storage/uploads";
            return Path.IsPathRooted(root) ? root : Path.Combine(environment.ContentRootPath, root);
        }
    }

    public async Task<string> SaveAsync(string originalFileName, string contentType, Stream content, CancellationToken ct = default)
    {
        Directory.CreateDirectory(RootPath);
        var ext = Path.GetExtension(originalFileName);
        if (string.IsNullOrWhiteSpace(ext) || ext.Length > 12) ext = ".bin";
        ext = ext.ToLowerInvariant();
        var stored = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}{ext}";
        var path = Path.Combine(RootPath, stored);
        await using (var fs = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true))
        {
            await content.CopyToAsync(fs, ct);
        }
        return stored;
    }

    public Task<Stream?> OpenReadAsync(string storedFileName, CancellationToken ct = default)
    {
        var path = GetPhysicalPath(storedFileName);
        if (!File.Exists(path)) return Task.FromResult<Stream?>(null);
        Stream s = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
        return Task.FromResult<Stream?>(s);
    }

    public Task DeleteAsync(string storedFileName, CancellationToken ct = default)
    {
        var path = GetPhysicalPath(storedFileName);
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }

    public string GetPhysicalPath(string storedFileName)
    {
        var safe = Path.GetFileName(storedFileName);
        return Path.Combine(RootPath, safe);
    }
}