#!/bin/bash

set -euo pipefail

# Change ownership of the .dotnet directory to the vscode user (to avoid permission errors)
sudo chown -R vscode:vscode /home/vscode/.aspnet
sudo chown -R vscode:vscode /home/vscode/.dotnet
sudo chown -R vscode:vscode /home/vscode/.dotnet/corefx/cryptography/x509stores
sudo chown -R vscode:vscode /home/vscode/.dotnet/tools
sudo chown -R vscode:vscode /home/vscode/.local
sudo chown -R vscode:vscode /home/vscode/.nuget
sudo chown -R vscode:vscode /home/vscode/.microsoft
sudo chown -R vscode:vscode /home/vscode/.microsoft/usersecrets

# Update the global workload manifest
sudo dotnet workload update

# Setup and trust the ASP.NET Core development certificate.
bash .devcontainer/configure-dev-certs.sh
