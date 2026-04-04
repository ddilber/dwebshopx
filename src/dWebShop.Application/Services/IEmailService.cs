namespace dWebShop.Application.Services;

public record OrderEmailItem(string Name, string SKU, decimal Quantity, decimal Price);

public interface IEmailService
{
    Task SendRegistrationNotificationToAdminAsync(string adminEmail, string userEmail, string userName);
    Task SendApprovalConfirmationToClientAsync(string clientEmail, string clientName);
    Task SendContactFormAsync(string adminEmail, string senderName, string senderEmail, string phone, string message);
    Task SendOrderConfirmationToClientAsync(string clientEmail, string clientName, Guid orderGuid, IEnumerable<OrderEmailItem> items, decimal total, string deliveryAddress);
    Task SendOrderConfirmationToAdminAsync(string adminEmail, string clientName, string clientEmail, Guid orderGuid, IEnumerable<OrderEmailItem> items, decimal total);
    Task SendOrderStatusChangedToClientAsync(string clientEmail, string clientName, Guid orderGuid, string newStatus);
}
