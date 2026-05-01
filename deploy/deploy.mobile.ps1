param(
    [string]$Profile = "preview",
    [string]$Platform = "android"
)

$appDir = "$PSScriptRoot\..\reqNot"

Write-Host "=== ReqNot Mobile Deploy ===" -ForegroundColor Cyan
Write-Host "Profile: $Profile | Platform: $Platform`n" -ForegroundColor Gray

$validProfiles = @("development", "preview", "production")
if ($Profile -notin $validProfiles) {
    Write-Host "Nieprawidlowy profil: $Profile. Dostepne: $($validProfiles -join ', ')" -ForegroundColor Red
    exit 1
}

Set-Location $appDir

Write-Host "Building with EAS..." -ForegroundColor Yellow
eas build --profile $Profile --platform $Platform
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed." -ForegroundColor Red
    exit 1
}

Write-Host "`nDone." -ForegroundColor Green
