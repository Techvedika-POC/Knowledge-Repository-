// Infrastructure/Services/SmtpEmailService.cs
using Knowledge_Repository.Application.Interfaces.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Knowledge_Repository.Infrastructure.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly string _host;
        private readonly int _port;
        private readonly string _user;
        private readonly string _pass;
        private readonly bool _useSsl;
        private readonly string _fromAddress;
        private readonly string _fromName;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IConfiguration config, ILogger<SmtpEmailService> logger)
        {
            _logger = logger;
            _host = config["Smtp:Host"] ?? throw new ArgumentNullException("Smtp:Host");
            _port = int.TryParse(config["Smtp:Port"], out var p) ? p : 587;
            _useSsl = bool.TryParse(config["Smtp:UseSsl"], out var s) ? s : true;
            _user = config["Smtp:User"];
            _pass = config["Smtp:Pass"];
            _fromAddress = config["Smtp:FromAddress"] ?? _user;
            _fromName = config["Smtp:FromDisplayName"] ?? "Knowledge Repo";
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
                throw new ArgumentException("toEmail is required", nameof(toEmail));

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_fromName, _fromAddress));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject ?? "(no subject)";

            var body = new BodyBuilder { HtmlBody = htmlBody ?? string.Empty };
            message.Body = body.ToMessageBody();

            using var client = new MailKit.Net.Smtp.SmtpClient();

            try
            {
                // choose secure socket option depending on port
                SecureSocketOptions sockOpt = SecureSocketOptions.StartTls;
                if (_port == 465) sockOpt = SecureSocketOptions.SslOnConnect;
                // If user set UseSsl = false, use StartTlsWhenAvailable (or None)
                if (!_useSsl) sockOpt = SecureSocketOptions.None;

                _logger?.LogDebug("Connecting SMTP {Host}:{Port}, Secure={Secure}", _host, _port, sockOpt);
                await client.ConnectAsync(_host, _port, sockOpt);

                if (!string.IsNullOrWhiteSpace(_user))
                {
                    _logger?.LogDebug("Authenticating SMTP user {User}", _user);
                    await client.AuthenticateAsync(_user, _pass);
                }

                await client.SendAsync(message);
                _logger?.LogInformation("Email sent to {To}", toEmail);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to send email to {To}", toEmail);
                throw; // bubble up so caller can log/track failure
            }
            finally
            {
                try { await client.DisconnectAsync(true); } catch { /* ignore */ }
            }
        }

        public async Task SendEmailBulkAsync(IEnumerable<string> toEmails, string subject, string htmlBody)
        {
            var recipients = toEmails?.Where(e => !string.IsNullOrWhiteSpace(e)).Distinct().ToList();
            if (recipients == null || recipients.Count == 0) return;

            // Option A: send one message with BCC — simpler but exposes emails to mail server
            // Option B: send individually (we do individually so logs show per-recipient)
            var tasks = recipients.Select(async to =>
            {
                try
                {
                    await SendEmailAsync(to, subject, htmlBody);
                }
                catch (Exception ex)
                {
                    // log but continue
                    _logger?.LogError(ex, "Failed to send bulk email to {To}", to);
                }
            });

            await Task.WhenAll(tasks);
        }
    }
}
