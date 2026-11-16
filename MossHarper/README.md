# Moss Harper website

.NET 10 Razor Pages site with an HTMX contact form. The contact handler sends server-side. Gmail SMTP is the selected provider for Julian's personal Gmail account; MailerSend remains available if he later verifies a domain he controls. Credentials never go to the browser.

## Run locally

```powershell
dotnet run --project MossHarper.csproj
```

For Gmail delivery, Julian must enable 2-Step Verification and create a Google app password for this site. Store only the app password in a private environment setting:

```text
GmailSmtp__AppPassword=the-generated-app-password
```

The Gmail username, Julian's recipient address, and David's temporary BCC test copy are in `appsettings.json`. Remove `ContactDelivery:TestCopyRecipients` after testing. Use a Google app password, never Julian's normal Gmail password. Google may not offer app passwords for accounts with Advanced Protection or some other security settings. The form reports success after Gmail's SMTP server accepts the message; that is not a final delivery guarantee.

If Julian later has a verified sending domain and chooses MailerSend, set `ContactDelivery__Provider=MailerSend`, `MailerSend__FromEmail` and `MailerSend__ApiToken`. The existing recipient and BCC settings remain in `ContactDelivery`.

## Linux VM

Build from this directory with `docker build -t moss-harper .`. Run the container behind a TLS-terminating reverse proxy, forwarding to port 8080. Supply the Gmail app password as a container environment variable or through a host-managed secret store. Do not commit credentials or an `.env` file. The same image can run on a DigitalOcean or Hetzner VM.

Set `ReverseProxy__KnownProxies__0` to the reverse proxy's IP address as seen by the app. The app will then use its forwarded scheme and client IP for HTTPS redirects and IP-based rate limiting. Mount a persistent, app-writable volume for ASP.NET Core Data Protection keys and set `DataProtection__KeysPath` to its container path. This keeps antiforgery tokens valid through restarts. Keep the app port private to the VM or container network; only the proxy should accept public traffic.

## Before public launch

- Verify the company name, number and registered-office wording against official records.
- Replace the pre-launch privacy information with an approved notice and confirmed contact details and response time.
- Replace `wwwroot/images/julian-harper-brown-temporary.jpg` with Julian Harper-Brown's final approved headshot and confirm its usage rights.
- Have Julian create a dedicated Google app password, store it privately on the server, and test end-to-end delivery before publishing the form.
- Review the building safety, tribunal and heat-network copy with the business owner.
