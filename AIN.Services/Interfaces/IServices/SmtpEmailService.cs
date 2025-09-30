using AIN.Application.Interfaces.IServices;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System.Threading;
using System.Threading.Tasks;

namespace AIN.Infrastructure.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public SmtpEmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendAsync(string to, string subject, string body, CancellationToken ct = default)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("AIN App", _config["Email:From"]));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            using var client = new SmtpClient();

            // Gmail prefers STARTTLS on port 587
            await client.ConnectAsync(
                _config["Email:SmtpHost"],
                int.Parse(_config["Email:Port"]),
                SecureSocketOptions.StartTls,
                ct);

            await client.AuthenticateAsync(
                _config["Email:From"],      // your Gmail address
                _config["Email:Password"],  // your App Password
                ct);

            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);
        }
    }
}
