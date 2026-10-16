#!/usr/bin/env bash
# Starts VitalHelse on http://localhost:1515 (needs the .NET 9 SDK or newer).
set -e
cd "$(dirname "$0")"

if ! command -v dotnet >/dev/null 2>&1; then
  echo "Fant ikke 'dotnet'. Installer .NET 9 SDK eller nyere: https://dotnet.microsoft.com/download" >&2
  exit 1
fi

export ASPNETCORE_ENVIRONMENT=Development
echo "Starter VitalHelse pa http://localhost:1515 (Ctrl+C for a avslutte)"
dotnet run --project vitalhelse/VitalHelse.csproj --urls http://localhost:1515
