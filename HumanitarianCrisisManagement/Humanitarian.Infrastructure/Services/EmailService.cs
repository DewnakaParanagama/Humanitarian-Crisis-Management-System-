using Humanitarian.Application.Interfaces;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Humanitarian.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendOtpEmailAsync(string email, string otp)
    {
        var smtpHost = _configuration["EmailSettings:SmtpHost"];
        var smtpPort = int.Parse(
            _configuration["EmailSettings:SmtpPort"] ?? "587");

        var senderEmail = _configuration["EmailSettings:SenderEmail"];
        var senderName = _configuration["EmailSettings:SenderName"];
        var username = _configuration["EmailSettings:Username"];
        var password = _configuration["EmailSettings:Password"];

        if (string.IsNullOrWhiteSpace(smtpHost) ||
            string.IsNullOrWhiteSpace(senderEmail) ||
            string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "Email SMTP configuration is incomplete.");
        }

        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(senderName, senderEmail));
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = "Sevana — Your Verification Code";

        message.Body = new TextPart("plain")
        {
            Text = $"""
                Hello,

                Your Sevana account verification code is:

                {otp}

                This verification code will expire in 10 minutes.

                Please do not share this code with anyone.

                Thank you,
                Sevana Team
                """
        };

        using var client = new SmtpClient();

        await client.ConnectAsync(
            smtpHost,
            smtpPort,
            SecureSocketOptions.StartTls);

        await client.AuthenticateAsync(username, password);

        await client.SendAsync(message);

        await client.DisconnectAsync(true);
    }
}