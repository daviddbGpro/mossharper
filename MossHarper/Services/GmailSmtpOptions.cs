namespace MossHarper.Services;

public sealed class GmailSmtpOptions
{
    public string Username { get; set; } = "";
    public string AppPassword { get; set; } = "";
    public int Port { get; set; } = 465;
}
