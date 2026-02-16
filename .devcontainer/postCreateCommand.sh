#!/bin/bash

# Setup the ASP.NET Code Generator tool
dotnet tool install --global dotnet-aspnet-codegenerator

# Setup the Entity Framework Core CLI tool
dotnet tool install --global dotnet-ef

# Install the Aspire Project Templates
dotnet new install Aspire.ProjectTemplates --force

# Install the Aspire CLI
curl -sSL https://aspire.dev/install.sh | bash

# Initialize Git LFS in the project directory this will fail during the build because the workspace
# is is still owned by the vscode user, but it will succeed when the container is started
cd /workspaces/Mystikweb.Auth.Demo/
git lfs update