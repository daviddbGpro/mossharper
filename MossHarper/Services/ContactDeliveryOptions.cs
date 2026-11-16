namespace MossHarper.Services;

public sealed class ContactDeliveryOptions
{
    public string Provider { get; set; } = "GmailSmtp";
    public string[] Recipients { get; set; } = [];
    public string[] TestCopyRecipients { get; set; } = [];
}
