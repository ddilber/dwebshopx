using dWebShop.Application.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace dWebShop.Infrastructure.Services;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public SmtpEmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendRegistrationNotificationToAdminAsync(string adminEmail, string userEmail, string userName)
    {
        var subject = "New client registration pending approval";
        var body = $"<p>A new client has registered and is awaiting your approval.</p>" +
                   $"<p><strong>Name:</strong> {userName}<br/>" +
                   $"<strong>Email:</strong> {userEmail}</p>" +
                   $"<p>Please log in to the admin panel to approve or reject this registration.</p>";

        await SendEmailAsync(adminEmail, subject, body);
    }

    public async Task SendApprovalConfirmationToClientAsync(string clientEmail, string clientName)
    {
        var subject = "Your account has been approved";
        var body = $"<p>Dear {clientName},</p>" +
                   $"<p>Your account has been approved. You can now log in to the portal.</p>";

        await SendEmailAsync(clientEmail, subject, body);
    }

    public async Task SendContactFormAsync(string adminEmail, string senderName, string senderEmail, string phone, string message)
    {
        var subject = $"Contact form message from {senderName}";
        var body = $"<p>A visitor has submitted the contact form.</p>" +
                   $"<p><strong>Name:</strong> {senderName}<br/>" +
                   $"<strong>Email:</strong> {senderEmail}<br/>" +
                   $"<strong>Phone:</strong> {(string.IsNullOrWhiteSpace(phone) ? "—" : phone)}</p>" +
                   $"<p><strong>Message:</strong><br/>{System.Net.WebUtility.HtmlEncode(message).Replace("\n", "<br/>")}</p>";

        await SendEmailAsync(adminEmail, subject, body);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        var host = _configuration["Email:Host"] ?? "localhost";
        var port = int.Parse(_configuration["Email:Port"] ?? "25");
        var username = _configuration["Email:Username"];
        var password = _configuration["Email:Password"];
        var senderEmail = _configuration["Email:SenderEmail"] ?? "noreply@dwebshop.local";
        var senderName = _configuration["Email:SenderName"] ?? "dWebShop";

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(senderName, senderEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();
        var secureSocketOptions = (port == 465)
            ? SecureSocketOptions.SslOnConnect
            : SecureSocketOptions.StartTlsWhenAvailable;

        await client.ConnectAsync(host, port, secureSocketOptions);

        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            await client.AuthenticateAsync(username, password);

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
