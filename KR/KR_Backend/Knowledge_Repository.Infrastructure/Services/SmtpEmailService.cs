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
        private readonly string? _user;
        private readonly string? _pass;
        private readonly bool _useSsl;
        private readonly string _fromAddress;
        private readonly string _fromName;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IConfiguration config, ILogger<SmtpEmailService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _host = config["Smtp:Host"] ?? throw new ArgumentNullException("Smtp:Host");
            _port = int.TryParse(config["Smtp:Port"], out var p) ? p : 587;
            _useSsl = bool.TryParse(config["Smtp:UseSsl"], out var s) ? s : true;
            _user = config["Smtp:User"];
            _pass = config["Smtp:Pass"];
            _fromAddress = config["Smtp:FromAddress"] ?? _user ?? throw new ArgumentNullException("Smtp:FromAddress or Smtp:User");
            _fromName = config["Smtp:FromDisplayName"] ?? "Knowledge Repo";
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            if (string.IsNullOrWhiteSpace(toEmail)) throw new ArgumentException("toEmail is required", nameof(toEmail));
            if (string.IsNullOrWhiteSpace(subject)) subject = "(no subject)";

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_fromName, _fromAddress));
            message.To.Add(MailboxAddress.Parse(toEmail.Trim()));

            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlBody ?? string.Empty,
                TextBody = StripHtml(htmlBody) 
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();

            try
            {
                await client.ConnectAsync(_host, _port, SecureSocketOptions.StartTls);


                if (!string.IsNullOrWhiteSpace(_user))
                {
                    _logger.LogDebug("SMTP authenticate {User}", _user);
                    await client.AuthenticateAsync(_user, _pass ?? string.Empty).ConfigureAwait(false);
                }

                await client.SendAsync(message).ConfigureAwait(false);
                _logger.LogInformation("Email sent to {To}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}", toEmail);
                throw; 
            }
            finally
            {
                try { await client.DisconnectAsync(true).ConfigureAwait(false); } catch { /* ignore */ }
            }
        }

        public async Task SendEmailBulkAsync(IEnumerable<string> toEmails, string subject, string htmlBody)
        {
            var recipients = toEmails?.Where(e => !string.IsNullOrWhiteSpace(e)).Select(e => e.Trim()).Distinct().ToList();
            if (recipients == null || recipients.Count == 0) return;

            var tasks = recipients.Select(to => Task.Run(async () =>
            {
                try
                {
                    await SendEmailAsync(to, subject, htmlBody).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send bulk email to {To}", to);
                }
            }));

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        public Task SendAsync(string to, string subject, string body) => SendEmailAsync(to, subject, body);
        private static string StripHtml(string? html)
        {
            if (string.IsNullOrEmpty(html)) return string.Empty;
            var sb = new System.Text.StringBuilder();
            var inside = false;
            foreach (var ch in html)
            {
                if (ch == '<') inside = true;
                else if (ch == '>') { inside = false; sb.Append(' '); }
                else if (!inside) sb.Append(ch);
            }
            return System.Text.RegularExpressions.Regex.Replace(sb.ToString(), @"\s+", " ").Trim();
        }
    }
}
