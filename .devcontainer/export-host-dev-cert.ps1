[CmdletBinding()]
param(
    [string]$OutputDirectory = "",
    [switch]$RotatePassword,
    [switch]$RepairTrust
)

$ErrorActionPreference = "Stop"

$scriptRoot = $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($scriptRoot) -and -not [string]::IsNullOrWhiteSpace($PSCommandPath)) {
    $scriptRoot = Split-Path -Parent $PSCommandPath
}

if ([string]::IsNullOrWhiteSpace($scriptRoot) -and $MyInvocation.MyCommand.Path) {
    $scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
}

if ([string]::IsNullOrWhiteSpace($scriptRoot)) {
    throw "Unable to determine the script directory for export-host-dev-cert.ps1. Run the script with -File from Windows PowerShell or use the WSL helper wrapper."
}

if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $OutputDirectory = Join-Path $scriptRoot "local/https"
}

if ($env:OS -ne "Windows_NT") {
    throw @"
This script must run on the Windows host because it needs access to the host .NET SDK and host certificate store.

If your repository lives in the WSL filesystem, run this script from Windows PowerShell by using the repo's \\wsl$ path,
or invoke the WSL helper script:

    bash .devcontainer/export-host-dev-cert-from-wsl.sh
"@
}

$certPath = Join-Path $OutputDirectory "aspnet-dev-cert.pfx"
$passwordPath = Join-Path $OutputDirectory "aspnet-dev-cert.password"
$thumbprintPath = Join-Path $OutputDirectory "aspnet-dev-cert.thumbprint"

function Test-IsWslSharePath([string]$Path) {
    return $Path.StartsWith('\\wsl$\', [System.StringComparison]::OrdinalIgnoreCase) -or
    $Path.StartsWith('\\wsl.localhost\', [System.StringComparison]::OrdinalIgnoreCase)
}

function Get-DevCertificateTrustStatus {
    $rawOutput = dotnet dev-certs https --check-trust-machine-readable

    if ([string]::IsNullOrWhiteSpace($rawOutput)) {
        return @()
    }

    $parsed = $rawOutput | ConvertFrom-Json
    if ($parsed -is [System.Array]) {
        return $parsed
    }

    return @($parsed)
}

New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null

if ($RepairTrust) {
    Write-Host "Repairing the host ASP.NET Core development certificate trust."
    Write-Host "Close all browser windows first so they reload trust state after the repair."

    dotnet dev-certs https --clean | Out-Null
    dotnet dev-certs https --trust | Out-Null

    $trustStatus = Get-DevCertificateTrustStatus
    if (-not ($trustStatus | Where-Object { $_.TrustLevel -eq "Full" })) {
        throw @"
Host development certificate trust is still not reported as Full.

If Chrome or Edge still warn after this repair:
- fully close all browser windows and retry
- confirm `dotnet dev-certs https --check --trust --verbose` shows a trusted certificate

If Firefox still warns:
- enable `security.enterprise_roots.enabled` in `about:config`, or
- import the current localhost development certificate into Firefox manually
"@
    }
}

if ($RotatePassword -or -not (Test-Path $passwordPath)) {
    $randomBytes = New-Object byte[] 48
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    try {
        $rng.GetBytes($randomBytes)
    }
    finally {
        if ($null -ne $rng) {
            $rng.Dispose()
        }
    }
    $password = [Convert]::ToBase64String($randomBytes)
    Set-Content -Path $passwordPath -Value $password -NoNewline
}
else {
    $password = (Get-Content -Path $passwordPath -Raw).Trim()
}

if ([string]::IsNullOrWhiteSpace($password)) {
    throw "The exported certificate password cannot be empty."
}

dotnet dev-certs https --trust | Out-Null

$trustStatus = Get-DevCertificateTrustStatus
$trustedCertificate = $trustStatus | Where-Object { $_.TrustLevel -eq "Full" } | Select-Object -First 1

if ($null -eq $trustedCertificate) {
    throw "Unable to find a fully trusted host ASP.NET development certificate to export."
}

if (Test-Path $certPath) {
    Remove-Item -Path $certPath -Force
}

$exportPath = $certPath
$stagedExportPath = $null

if (Test-IsWslSharePath $certPath) {
    $stagedExportPath = Join-Path ([System.IO.Path]::GetTempPath()) ("aspnet-dev-cert-{0}.pfx" -f [System.Guid]::NewGuid().ToString("N"))
    $exportPath = $stagedExportPath
}

try {
    dotnet dev-certs https --export-path $exportPath --password $password | Out-Null

    if (-not (Test-Path $exportPath)) {
        throw "The ASP.NET development certificate export did not produce a PFX file at '$exportPath'."
    }

    $exportedFile = Get-Item -Path $exportPath
    if ($exportedFile.Length -le 0) {
        throw "The ASP.NET development certificate export created an empty PFX file at '$exportPath'."
    }

    if ($exportPath -ne $certPath) {
        Copy-Item -Path $exportPath -Destination $certPath -Force
    }
}
finally {
    if ($null -ne $stagedExportPath -and (Test-Path $stagedExportPath)) {
        Remove-Item -Path $stagedExportPath -Force
    }
}

if (-not (Test-Path $certPath)) {
    throw "The ASP.NET development certificate PFX file was not written to '$certPath'."
}

$finalExportedFile = Get-Item -Path $certPath
if ($finalExportedFile.Length -le 0) {
    throw "The ASP.NET development certificate PFX file at '$certPath' is empty."
}

Set-Content -Path $thumbprintPath -Value $trustedCertificate.Thumbprint -NoNewline

Write-Host "Exported host ASP.NET development certificate to: $certPath"
Write-Host "Password file written to: $passwordPath"
Write-Host "Thumbprint file written to: $thumbprintPath"
if ($RepairTrust) {
    Write-Host "Host trust was repaired before export."
}
Write-Host "Rebuild or reopen the dev container, or run 'bash .devcontainer/configure-dev-certs.sh' inside the container to import it."