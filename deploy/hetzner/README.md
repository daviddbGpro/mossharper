# Hetzner deployment

This deployment runs the .NET site behind Caddy on the CPX12 Ubuntu server. Caddy handles HTTPS for `mossharper.com`. The ASP.NET Core Data Protection keys and TLS certificates persist in Docker volumes. The app itself is only reachable through Caddy.

## Before starting

- Confirm `mossharper.com` has an A record pointing to the server IPv4 address.
- Open inbound TCP ports 80 and 443 in the Hetzner Cloud Firewall. Allow SSH (22) from your own IP address. Do not expose port 8080.
- Install Docker Engine and the Docker Compose plugin using the [official Ubuntu instructions](https://docs.docker.com/engine/install/ubuntu/). The included `bootstrap.sh` applies those instructions on a fresh Ubuntu server after cloning the repository; run it as root.
- Have Julian's Gmail app password ready. Do not put it in Git or send it in chat.

## First deployment

Clone `https://github.com/daviddbGpro/mossharper.git` to a directory on the server, then run these commands from the repository root:

```bash
cp deploy/hetzner/example.env deploy/hetzner/.env
chmod 600 deploy/hetzner/.env
nano deploy/hetzner/.env
docker compose -f deploy/hetzner/compose.yaml up -d --build
docker compose -f deploy/hetzner/compose.yaml ps
```

Enter the app password as `GmailSmtp__AppPassword` in `.env`. The Compose file selects Gmail port 587 with STARTTLS. Wait for Caddy to obtain the HTTPS certificate, then open `https://mossharper.com` and test the contact form. Check Julian's inbox and the app logs:

```bash
docker compose -f deploy/hetzner/compose.yaml logs --tail=100 app caddy
```

## Updates

From the repository root on the server:

```bash
git pull --ff-only
docker compose -f deploy/hetzner/compose.yaml up -d --build
```

The `.env` file and Docker volumes stay on the server during a normal rebuild. Back up the server, including the Docker volumes. Keep SnapDeploy running until the new site and contact mail are confirmed working.
