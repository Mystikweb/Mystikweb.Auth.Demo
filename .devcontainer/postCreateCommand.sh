#!/bin/bash

set -euo pipefail

# Setup the ASP.NET Code Generator tool
dotnet tool install --global dotnet-aspnet-codegenerator

# Setup the Entity Framework Core CLI tool
dotnet tool install --global dotnet-ef

# Install the Aspire Project Templates
dotnet new install Aspire.ProjectTemplates --force

# Install the Fluent UI ASP.NET Core Templates
dotnet new install Microsoft.FluentUI.AspNetCore.Templates --force

# Install Aspire CLI
curl -sSL https://aspire.dev/install.sh | bash

# Install Playwright separately so it can be rerun after package version updates.
# bash .devcontainer/install-playwright.sh