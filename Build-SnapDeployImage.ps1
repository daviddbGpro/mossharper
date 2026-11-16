$ErrorActionPreference = 'Stop'

$projectDir = Join-Path $PSScriptRoot 'MossHarper'
$archivePath = Join-Path $PSScriptRoot 'MossHarper-arm64.tar'

Push-Location $projectDir
try {
    dotnet publish MossHarper.csproj `
        --configuration Release `
        --os linux `
        --arch arm64 `
        --self-contained false `
        -t:PublishContainer `
        -p:ContainerImageFormat=Docker `
        -p:ContainerRepository=moss-harper `
        -p:ContainerImageTag=testing `
        -p:ContainerPort=8080 `
        "-p:ContainerArchiveOutputPath=$archivePath"

    if ($LASTEXITCODE -ne 0) {
        throw "Container publish failed with exit code $LASTEXITCODE."
    }
}
finally {
    Pop-Location
}

Get-Item -LiteralPath $archivePath | Select-Object FullName, Length, LastWriteTime
