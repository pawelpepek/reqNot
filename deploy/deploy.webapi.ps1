$settingsFile = "$PSScriptRoot\deploy.settings.json"
$projectRoot  = "$PSScriptRoot\.."
$publishDir   = "$projectRoot\publish\webapi"
$RemotePath   = "/opt/reqnot"

Write-Host "=== ReqNot WebApi Deploy ===" -ForegroundColor Cyan

# Load settings
$Server = ""
$User   = ""
if (Test-Path $settingsFile) {
    $settings = Get-Content $settingsFile | ConvertFrom-Json
    $Server   = $settings.Server
    $User     = $settings.User
}

if (-not $Server) { $Server = Read-Host "Adres serwera" }
if (-not $User)   { $User   = Read-Host "Uzytkownik SSH" }

# Build
Write-Host "`nBuilding..." -ForegroundColor Yellow
dotnet publish "$projectRoot\ReqNot.WebApi" -c Release -r linux-arm64 --self-contained false -o $publishDir
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed." -ForegroundColor Red
    exit 1
}
Write-Host "Build OK." -ForegroundColor Green

# Stop service
Write-Host "`nStopping service..." -ForegroundColor Yellow
ssh "${User}@${Server}" "sudo systemctl stop reqnot-webapi"

# Copy files (via /tmp to avoid Windows glob issues with scp)
Write-Host "`nCopying files..." -ForegroundColor Yellow
scp -r "${publishDir}" "${User}@${Server}:/tmp/reqnot_deploy"
if ($LASTEXITCODE -ne 0) {
    Write-Host "Copy failed." -ForegroundColor Red
    exit 1
}
ssh "${User}@${Server}" "sudo cp -r /tmp/reqnot_deploy/* ${RemotePath}/ && rm -rf /tmp/reqnot_deploy"
if ($LASTEXITCODE -ne 0) {
    Write-Host "Move failed." -ForegroundColor Red
    exit 1
}
Write-Host "Copy OK." -ForegroundColor Green

# Start service and show status
Write-Host "`nStarting service..." -ForegroundColor Yellow
ssh "${User}@${Server}" "sudo systemctl start reqnot-webapi && sudo systemctl status reqnot-webapi --no-pager"

Write-Host "`nDone." -ForegroundColor Green
