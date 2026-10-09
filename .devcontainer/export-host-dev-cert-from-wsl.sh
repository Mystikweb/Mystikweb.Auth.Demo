#!/usr/bin/env bash

set -euo pipefail

if [[ "${EUID:-$(id -u)}" -eq 0 ]]; then
    echo "Do not run this script with sudo. It needs normal WSL-to-Windows interop from your user shell." >&2
    exit 1
fi

if ! command -v wslpath >/dev/null 2>&1; then
    echo "This script must run from a host WSL shell, not from the devcontainer." >&2
    echo "'wslpath' is not available in the current shell." >&2
    exit 1
fi

SCRIPT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
POWERSHELL_SCRIPT_PATH=$(wslpath -w "$SCRIPT_DIR/export-host-dev-cert.ps1")

POWERSHELL_HOST=""
if command -v powershell.exe >/dev/null 2>&1; then
    POWERSHELL_HOST="powershell.exe"
elif command -v pwsh.exe >/dev/null 2>&1; then
    POWERSHELL_HOST="pwsh.exe"
elif [[ -x /mnt/c/Windows/System32/WindowsPowerShell/v1.0/powershell.exe ]]; then
    POWERSHELL_HOST="/mnt/c/Windows/System32/WindowsPowerShell/v1.0/powershell.exe"
elif [[ -x "/mnt/c/Program Files/PowerShell/7/pwsh.exe" ]]; then
    POWERSHELL_HOST="/mnt/c/Program Files/PowerShell/7/pwsh.exe"
fi

if [[ -z "$POWERSHELL_HOST" ]]; then
    echo "A Windows PowerShell host was not found from this WSL shell." >&2
    echo "Install Windows PowerShell or PowerShell 7 on the host, or run the export script directly from a Windows PowerShell window." >&2
    exit 1
fi

"$POWERSHELL_HOST" -NoProfile -ExecutionPolicy Bypass -File "$POWERSHELL_SCRIPT_PATH" "$@"