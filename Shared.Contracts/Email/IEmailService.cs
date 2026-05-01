namespace Shared.Contracts.Email;

public record MailAttachment(string FileName, byte[] Content, string ContentType);

public interface IEmailService
{
    Task SendAsync(
        string to,
        string subject,
        string htmlBody,
        List<MailAttachment>? attachments = null,
        CancellationToken cancellationToken = default);
}
