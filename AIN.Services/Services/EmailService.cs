using AIN.Application.Interfaces.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIN.Application.Services
{
    public class EmailService : IEmailService
    {
        public Task SendAsync(string to, string subject, string body, CancellationToken ct = default)
        {
            // For now just log
            Console.WriteLine($"Sending email to {to}");
            Console.WriteLine($"Subject: {subject}");
            Console.WriteLine($"Body: {body}");
            return Task.CompletedTask;

        }
    }
}