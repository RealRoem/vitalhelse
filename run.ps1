# Starts VitalHelse on http://localhost:1515 (needs the .NET 9 SDK or newer).
Set-Location $PSScriptRoot
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Error "Fant ikke 'dotnet'. Installer .NET 9 SDK eller nyere: https://dotnet.microsoft.com/download"
    exit 1
}
$env:ASPNETCORE_ENVIRONMENT = "Development"
Write-Host "Starter VitalHelse pa http://localhost:1515 (Ctrl+C for a avslutte)"
dotnet run --project vitalhelse/VitalHelse.csproj --urls http://localhost:1515
