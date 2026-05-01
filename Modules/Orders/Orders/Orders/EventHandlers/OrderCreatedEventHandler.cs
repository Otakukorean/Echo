using Microsoft.Extensions.Logging;
using Orders.Orders.Events;
using Shared.Contracts.Email;
using Shared.Contracts.Pdf;

namespace Orders.Orders.EventHandlers;

public class OrderCreatedEventHandler(
    IEmailService emailService,
    IPdfService pdfService,
    ILogger<OrderCreatedEventHandler> logger) : INotificationHandler<OrderCreatedEvent>
{
    public async Task Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Order created: {OrderNumber}, sending receipt to {Email}",
            notification.OrderNumber, notification.CustomerEmail);

        // Generate PDF receipt
        var pdfBytes = pdfService.Generate(pdf =>
        {
            pdf.AddTitle("Order Receipt");
            pdf.AddSubtitle($"Order #{notification.OrderNumber}");
            pdf.AddSpacing(5);
            pdf.AddText($"Customer: {notification.CustomerName}");
            pdf.AddText($"Email: {notification.CustomerEmail}");
            pdf.AddSpacing(15);

            pdf.AddTable(
                ["Product", "Qty", "Unit Price", "Total"],
                notification.Items.Select(i => new[]
                {
                    i.ProductName,
                    i.Quantity.ToString(),
                    $"${i.UnitPrice:F2}",
                    $"${i.LineTotal:F2}"
                }).ToList());

            pdf.AddTotalLine("Total", $"${notification.Total:F2}");
            pdf.AddFooter("Thank you for your order!");
        });

        // Send email with PDF attachment
        var htmlBody = $"""
            <h1>Thank you for your order!</h1>
            <p>Hi {notification.CustomerName},</p>
            <p>Your order <strong>#{notification.OrderNumber}</strong> has been received.</p>
            <p>Total: <strong>${notification.Total:F2}</strong></p>
            <p>Your receipt is attached as a PDF.</p>
            <br/>
            <p>— Echo</p>
            """;

        var attachments = new List<MailAttachment>
        {
            new($"receipt-{notification.OrderNumber}.pdf", pdfBytes, "application/pdf")
        };

        try
        {
            await emailService.SendAsync(
                notification.CustomerEmail,
                $"Order Confirmation - {notification.OrderNumber}",
                htmlBody,
                attachments,
                cancellationToken);

            logger.LogInformation("Receipt email sent for order {OrderNumber}", notification.OrderNumber);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send receipt email for order {OrderNumber}", notification.OrderNumber);
            // Don't throw — the order is already saved, email failure shouldn't roll it back
        }
    }
}
