namespace MossHarper.Services;

public sealed class MailerSendOptions
{
    public string ApiToken { get; set; } = "";
    public string FromEmail { get; set; } = "";
    public string FromName { get; set; } = "Moss Harper";
}
