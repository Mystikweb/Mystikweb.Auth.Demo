#!/bin/bash

set -euo pipefail

SCRIPT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
CERT_EXPORT_PATH="/usr/local/share/ca-certificates/dotnet-dev-cert.crt"
TRUST_DIR="/home/vscode/.aspnet/dev-certs/trust"
SSL_CERT_DIR_VALUE="$TRUST_DIR:/usr/lib/ssl/certs"
HOST_CERT_DIR="$SCRIPT_DIR/local/https"
HOST_CERT_PFX="$HOST_CERT_DIR/aspnet-dev-cert.pfx"
HOST_CERT_PASSWORD_FILE="$HOST_CERT_DIR/aspnet-dev-cert.password"
HOST_CERT_THUMBPRINT_FILE="$HOST_CERT_DIR/aspnet-dev-cert.thumbprint"

mkdir -p "$TRUST_DIR"
echo "export SSL_CERT_DIR=\"$SSL_CERT_DIR_VALUE\"" | sudo tee /etc/profile.d/dotnet-dev-certs.sh >/dev/null
sudo chmod 644 /etc/profile.d/dotnet-dev-certs.sh
export SSL_CERT_DIR="$SSL_CERT_DIR_VALUE"

imported_host_cert=false
expected_host_thumbprint=""

if [[ -f "$HOST_CERT_THUMBPRINT_FILE" ]]; then
    expected_host_thumbprint=$(tr -d '\r\n' < "$HOST_CERT_THUMBPRINT_FILE")
fi

host_export_files_present=0
[[ -f "$HOST_CERT_PFX" ]] && ((host_export_files_present += 1))
[[ -f "$HOST_CERT_PASSWORD_FILE" ]] && ((host_export_files_present += 1))
[[ -f "$HOST_CERT_THUMBPRINT_FILE" ]] && ((host_export_files_present += 1))

if (( host_export_files_present > 0 && host_export_files_present < 3 )); then
    echo "The host development certificate export is incomplete." >&2
    echo "Expected these files under $HOST_CERT_DIR:" >&2
    echo "- $(basename "$HOST_CERT_PFX")" >&2
    echo "- $(basename "$HOST_CERT_PASSWORD_FILE")" >&2
    echo "- $(basename "$HOST_CERT_THUMBPRINT_FILE")" >&2
    echo "Re-run the host export script before retrying container certificate setup." >&2
    exit 1
fi

if [[ -f "$HOST_CERT_PFX" && -f "$HOST_CERT_PASSWORD_FILE" ]]; then
    host_cert_password=$(tr -d '\r\n' < "$HOST_CERT_PASSWORD_FILE")

    if [[ -n "$host_cert_password" ]]; then
        echo "Importing host ASP.NET development certificate from $HOST_CERT_PFX"
        echo "Clearing existing container development certificates and importing the host certificate"
        dotnet dev-certs https --clean --import "$HOST_CERT_PFX" -p "$host_cert_password"
        dotnet dev-certs https --trust

        if [[ -n "$expected_host_thumbprint" ]]; then
            imported_thumbprint=$(dotnet dev-certs https --check-trust-machine-readable | grep -oP '"Thumbprint":\s*"\K[^"]+' | head -n 1)

            if [[ -z "$imported_thumbprint" ]]; then
                echo "Unable to read the imported container certificate thumbprint after host certificate import." >&2
                exit 1
            fi

            if [[ "$imported_thumbprint" != "$expected_host_thumbprint" ]]; then
                echo "Imported container certificate thumbprint does not match the exported host certificate." >&2
                echo "Expected: $expected_host_thumbprint" >&2
                echo "Actual:   $imported_thumbprint" >&2
                echo "Re-export the host certificate and retry, or inspect whether the repository-local export files are stale." >&2
                echo "If this persists across container rebuilds, remember that the devcontainer certificate volumes are named Docker volumes and are not cleared by a rebuild without cache." >&2
                exit 1
            fi
        fi

        imported_host_cert=true
    else
        echo "Host development certificate password file is empty; falling back to container-managed certificate." >&2
    fi
fi

if [[ "$imported_host_cert" != "true" ]]; then
    if ! dotnet dev-certs https --check --trust >/dev/null 2>&1; then
        dotnet dev-certs https --clean || true
        dotnet dev-certs https --trust
    fi
fi

sudo -E dotnet dev-certs https --export-path "$CERT_EXPORT_PATH" --format pem
sudo update-ca-certificates