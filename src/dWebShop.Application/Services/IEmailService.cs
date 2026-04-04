namespace dWebShop.Application.Services;

public interface IEmailService
{
    Task SendRegistrationNotificationToAdminAsync(string adminEmail, string userEmail, string userName);
    Task SendApprovalConfirmationToClientAsync(string clientEmail, string clientName);
    Task SendContactFormAsync(string adminEmail, string senderName, string senderEmail, string phone, string message);
}
