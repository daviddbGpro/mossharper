using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace MossHarper.Services;

public sealed class MailerSendContactMailer(
    HttpClient httpClient,
    IOptions<MailerSendOptions> options,
    IOptions<ContactDeliveryOptions> deliveryOptions,
    ILogger<MailerSendContactMailer> logger) : IContactMailer
{
    public async Task<bool> SendAsync(string name, string email, string organisation, string message, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var delivery = deliveryOptions.Value;
        if (string.IsNullOrWhiteSpace(settings.ApiToken) ||
            string.IsNullOrWhiteSpace(settings.FromEmail) ||
            delivery.Recipients.Length == 0)
        {
            logger.LogError("MailerSend contact settings are incomplete.");
            return false;
        }

        var body = new Dictionary<string, object>
        {
            ["from"] = new { email = settings.FromEmail, name = settings.FromName },
            ["to"] = delivery.Recipients.Select(address => new { email = address.Trim() }).ToArray(),
            ["reply_to"] = new { email, name },
            ["subject"] = "Website enquiry | Moss Harper",
            ["text"] = $"Name: {name}\nEmail: {email}\nOrganisation: {organisation}\n\nMessage:\n{message}"
        };
        if (delivery.TestCopyRecipients.Length > 0)
            body["bcc"] = delivery.TestCopyRecipients.Select(address => new { email = address.Trim() }).ToArray();

        using var request = new HttpRequestMessage(HttpMethod.Post, "v1/email")
        {
            Content = JsonContent.Create(body)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiToken);

        try
        {
            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (response.StatusCode == HttpStatusCode.Accepted &&
                response.Headers.TryGetValues("x-message-id", out _))
            {
                return true;
            }

            logger.LogError("MailerSend rejected a contact enquiry with status {StatusCode}.", (int)response.StatusCode);
            return false;
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            logger.LogError(exception, "MailerSend contact delivery failed.");
            return false;
        }
    }
}
