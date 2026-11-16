using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace MossHarper.Services;

public sealed class GmailSmtpContactMailer(
    IOptions<GmailSmtpOptions> gmailOptions,
    IOptions<ContactDeliveryOptions> deliveryOptions,
    ILogger<GmailSmtpContactMailer> logger) : IContactMailer
{
    public async Task<bool> SendAsync(string name, string email, string organisation, string message, CancellationToken cancellationToken)
    {
        var gmail = gmailOptions.Value;
        var delivery = deliveryOptions.Value;
        if (string.IsNullOrWhiteSpace(gmail.Username) ||
            string.IsNullOrWhiteSpace(gmail.AppPassword) ||
            delivery.Recipients.Length == 0)
        {
            logger.LogError("Gmail contact settings are incomplete.");
            return false;
        }

        try
        {
            var mail = new MimeMessage();
            mail.From.Add(new MailboxAddress("Moss Harper", gmail.Username.Trim()));
            foreach (var recipient in delivery.Recipients)
                mail.To.Add(new MailboxAddress("", recipient.Trim()));
            foreach (var copyRecipient in delivery.TestCopyRecipients)
                mail.Bcc.Add(new MailboxAddress("", copyRecipient.Trim()));
            mail.ReplyTo.Add(new MailboxAddress(name, email));
            mail.Subject = "Website enquiry | Moss Harper";
            mail.Body = new TextPart("plain")
            {
                Text = $"Name: {name}\nEmail: {email}\nOrganisation: {organisation}\n\nMessage:\n{message}"
            };

            using var client = new SmtpClient { Timeout = 15000 };
            await client.ConnectAsync("smtp.gmail.com", 465, SecureSocketOptions.SslOnConnect, cancellationToken);
            await client.AuthenticateAsync(gmail.Username.Trim(), gmail.AppPassword.Replace(" ", ""), cancellationToken);
            await client.SendAsync(mail, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
            return true;
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogError(exception, "Gmail contact delivery failed.");
            return false;
        }
    }
}
