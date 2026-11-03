using JobRadar.Core.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace JobRadar.Cli.Notifications;

public sealed class EmailNotifier
{
    private readonly EmailOptions _options;

    public EmailNotifier(EmailOptions options)
    {
        _options = options;
    }

    public async Task SendAsync(IReadOnlyList<JobPost> jobs, string htmlBody, CancellationToken ct = default)
    {
        if (jobs.Count == 0 && _options.SendOnlyIfNew)
            return;

        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(_options.From));
        message.To.Add(MailboxAddress.Parse(_options.To));
        message.Subject = $"JobRadar – {jobs.Count} neue Stelle(n) · {DateTime.Now:yyyy-MM-dd}";

        message.Body = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();
        await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls, ct);
        // App password – never log or print
        await client.AuthenticateAsync(_options.From, _options.AppPassword, ct);
        await client.SendAsync(message, ct);
        await client.DisconnectAsync(quit: true, ct);
    }
}
