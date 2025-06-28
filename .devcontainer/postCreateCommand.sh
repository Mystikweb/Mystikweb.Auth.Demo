#!/bin/bash

# Setup the ASP.NET Code Generator tool
dotnet tool install --global dotnet-aspnet-codegenerator

# Setup the Entity Framework Core CLI tool
dotnet tool install --global dotnet-ef

# Install the Aspire Project Templates
dotnet new install Aspire.ProjectTemplates --force

# Change ownership of the project directory to the container user (to avoid permission errors)
sudo chown -R authdemodev:authdemodev /workspaces/Mystikweb.Auth.Demo/

# Initialize Git LFS in the project directory this will fail during the build because the workspace
# is is still owned by the vscode user, but it will succeed when the container is started
cd /workspaces/Mystikweb.Auth.Demo/
git lfs update