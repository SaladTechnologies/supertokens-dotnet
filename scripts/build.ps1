#!/usr/bin/env pwsh
[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# Build Docker images
$ProjectRoot = Split-Path -Path $PSScriptRoot -Parent
Push-Location -Path $ProjectRoot
try {
    & dotnet tool restore
    & dotnet paket restore
    & dotnet restore .\supertokens-dotnet.sln
    & dotnet build --configuration Release --no-dependencies --no-restore .\src\SuperTokens.Net\SuperTokens.Net.csproj
    & dotnet build --configuration Release --no-dependencies --no-restore .\src\SuperTokens.AspNetCore\SuperTokens.AspNetCore.csproj
    & dotnet pack --configuration Release --no-build --no-dependencies --no-restore --output "$(Join-Path -Path $ProjectRoot -ChildPath artifacts)" .\src\SuperTokens.Net\SuperTokens.Net.csproj
    & dotnet pack --configuration Release --no-build --no-dependencies --no-restore --output "$(Join-Path -Path $ProjectRoot -ChildPath artifacts)" .\src\SuperTokens.AspNetCore\SuperTokens.AspNetCore.csproj
}
finally {
    Pop-Location
}
