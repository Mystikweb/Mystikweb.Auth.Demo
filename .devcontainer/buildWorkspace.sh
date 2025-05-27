#!/bin/bash

# Create the folders required for ASP.NET Core
mkdir -p /home/vscode/.aspnet

# Create the folders required for global dotnet CLI tools
mkdir -p /home/vscode/.dotnet
mkdir -p /home/vscode/.dotnet/tools

# Create the folders required for managing user local tools
mkdir -p /home/vscode/.local
mkdir -p /home/vscode/.local/share
mkdir -p /home/vscode/.local/share/man
mkdir -p /home/vscode/.local/share/NuGet
mkdir -p /home/vscode/.local/share/NuGet/http-cache

# Create the folders required for managing NuGet packages
mkdir -p /home/vscode/.nuget

# Create the folders required for managing project user secrets
mkdir -p /home/vscode/.microsoft
mkdir -p /home/vscode/.microsoft/usersecrets