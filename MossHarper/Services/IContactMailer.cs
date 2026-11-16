namespace MossHarper.Services;

public interface IContactMailer
{
    Task<bool> SendAsync(string name, string email, string organisation, string message, CancellationToken cancellationToken);
}
