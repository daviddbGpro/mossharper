# SnapDeploy test deployment

## Deploy from GitHub

The repository root contains `MossHarper.sln`; the deployable app and its Dockerfile are in the `MossHarper` folder. The root `.gitignore` keeps the Docker archive, source ZIP, review PDF, build output and local secrets out of Git.

1. Create a private GitHub repository and push this workspace to its `main` branch. Keep `GmailSmtp__AppPassword` out of GitHub.
2. In SnapDeploy, connect GitHub, select that repository and `main`, and set **Root directory** to `MossHarper`. SnapDeploy will use `MossHarper/Dockerfile` and build a Linux ARM64 image. If the dashboard offers linking the repository to the existing container, use that to retain its address; otherwise create a new container from the repository.
3. Set the container port to **8080** if it is not detected from the Dockerfile.
4. Set `GmailSmtp__AppPassword` in the container's runtime environment settings. The sender and Julian's recipient address are in `appsettings.json`. Do not put the app password in the repository or Dockerfile.
5. Deploy and open the Contact page. Submit a test enquiry, then check Julian's and David's inboxes and spam folders. If the form reports a delivery error, inspect the container logs for `Gmail contact delivery failed.`

Each push to the linked branch starts a new deployment and counts toward the free deploy quota. A GitHub deployment still sleeps on the free tier and still has no persistent disk; GitHub provides a repeatable build source, not an always-running container. If you need continuous uptime, SnapDeploy's Always-On plan or a Linux VM is required.

## Legacy image-upload method

## Upload the built Docker image

Use `MossHarper-arm64.tar` from the workspace root. It is a Docker-format image archive containing `moss-harper:testing` for Linux ARM64. It has a `manifest.json` and exposes port 8080. Do **not** select `MossHarper-SnapDeploy-source.zip` in SnapDeploy's **Upload a Docker image** screen; that ZIP contains source code, not a `docker save` image.

1. In SnapDeploy, choose **Deploy** then **Upload a Docker image**.
2. Select `MossHarper-arm64.tar`.
3. Choose a Small container for initial testing and set the container port to **8080** if it is not detected from `EXPOSE 8080`.
4. Add `GmailSmtp__AppPassword` as a runtime secret once Julian has created a Google app password. Do not put it in the image, ZIP or Dockerfile. The sender and Julian's recipient address are already in `appsettings.json`.
5. Deploy, open the HTTPS address provided by SnapDeploy, and submit a test enquiry. Check Julian's inbox. If sending fails, inspect the container logs.

The website itself can be previewed before the Gmail password is set, but the contact form will show a delivery error until the password is present. SnapDeploy permits outbound TCP port 465, which the app uses for Gmail SMTP. An end-to-end test is still needed to confirm Gmail accepts this host.

Free containers sleep after about 15 minutes without traffic and wake on a browser visit, typically in about a minute. The free filesystem does not persist through sleep or restart. If a contact form was left open across a sleep, reload it before submitting so its antiforgery token is fresh. Free containers use a SnapDeploy subdomain; a custom domain requires Always-On.

## Rebuild the archive without Docker

From the workspace root, run `./Build-SnapDeployImage.ps1` in PowerShell. This rebuilds `MossHarper-arm64.tar` from the current project source. From the `MossHarper` project directory, the equivalent .NET 10 SDK command is:

```powershell
dotnet publish MossHarper.csproj --configuration Release --os linux --arch arm64 --self-contained false -t:PublishContainer -p:ContainerImageFormat=Docker -p:ContainerRepository=moss-harper -p:ContainerImageTag=testing -p:ContainerPort=8080 -p:ContainerArchiveOutputPath=../MossHarper-arm64.tar
```

The repository also has a conventional `Dockerfile`. If Docker is installed later, these commands from the project directory produce the same type of upload file:

```powershell
docker build --platform linux/arm64 -t moss-harper:testing .
docker save -o ..\MossHarper-arm64.tar moss-harper:testing
```

SnapDeploy requires uploaded images to be ARM64. Its free image-upload limit is 2 GB.

The .NET SDK archive builder uses the project files, not the Dockerfile. Changes to Razor pages, C# code, CSS, images or `appsettings.json` are included when you rerun the script. If you change the Dockerfile itself, use the Docker commands instead. Runtime secrets such as `GmailSmtp__AppPassword` belong in SnapDeploy's environment settings and do not require a new archive.
