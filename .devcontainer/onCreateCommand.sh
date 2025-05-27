#!/bin/bash

# Change ownership of the .dotnet directory to the vscode user (to avoid permission errors)
sudo chown -R vscode:vscode /home/vscode/.aspnet
sudo chown -R vscode:vscode /home/vscode/.dotnet
sudo chown -R vscode:vscode /home/vscode/.dotnet/tools
sudo chown -R vscode:vscode /home/vscode/.local
sudo chown -R vscode:vscode /home/vscode/.nuget
sudo chown -R vscode:vscode /home/vscode/.microsoft
sudo chown -R vscode:vscode /home/vscode/.microsoft/usersecrets

# Setup the .NET Dev Certificates
sudo dotnet dev-certs https

# Export the ASP.NET Core HTTPS development certificate to a PEM file
sudo -E dotnet dev-certs https --export-path /usr/local/share/ca-certificates/dotnet-dev-cert.crt --format pem

# Add the PEM file to the trust store
sudo update-ca-certificates

# Update the global workload manifest
sudo dotnet workload update