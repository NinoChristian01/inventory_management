namespace ServiceDesk.Application.Interfaces;

public interface IAttachmentStorage
{
    Task<string> SaveAsync(string originalFileName, string contentType, Stream content, CancellationToken ct = default);
    Task<Stream?> OpenReadAsync(string storedFileName, CancellationToken ct = default);
    Task DeleteAsync(string storedFileName, CancellationToken ct = default);
    string GetPhysicalPath(string storedFileName);
}