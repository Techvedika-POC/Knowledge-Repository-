using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlBody);
        Task SendEmailBulkAsync(IEnumerable<string> toEmails, string subject, string htmlBody);

        Task SendAsync(string to, string subject, string body);
    }
}
