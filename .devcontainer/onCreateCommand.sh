#!/bin/bash

# Change ownership of the .dotnet directory to the vscode user (to avoid permission errors)
sudo chown -R authdemodev:authdemodev /home/authdemodev/.aspnet
sudo chown -R authdemodev:authdemodev /home/authdemodev/.dotnet
sudo chown -R authdemodev:authdemodev /home/authdemodev/.dotnet/corefx/cryptography/x509stores/my
sudo chown -R authdemodev:authdemodev /home/authdemodev/.dotnet/tools
sudo chown -R authdemodev:authdemodev /home/authdemodev/.local
sudo chown -R authdemodev:authdemodev /home/authdemodev/.nuget
sudo chown -R authdemodev:authdemodev /home/authdemodev/.microsoft
sudo chown -R authdemodev:authdemodev /home/authdemodev/.microsoft/usersecrets

# Setup the .NET Dev Certificates
sudo dotnet dev-certs https

# Export the ASP.NET Core HTTPS development certificate to a PEM file
sudo -E dotnet dev-certs https --export-path /usr/local/share/ca-certificates/dotnet-dev-cert.crt --format pem

# Add the PEM file to the trust store
sudo update-ca-certificates

# Update the global workload manifest
sudo dotnet workload update