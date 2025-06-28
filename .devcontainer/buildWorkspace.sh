#!/bin/bash

# Create the folders required for ASP.NET Core
mkdir -p /home/authdemodev/.aspnet

# Create the folders required for global dotnet CLI tools
mkdir -p /home/authdemodev/.dotnet
mkdir -p /home/authdemodev/.dotnet/tools

# Create the folders required for managing user local tools
mkdir -p /home/authdemodev/.local
mkdir -p /home/authdemodev/.local/share
mkdir -p /home/authdemodev/.local/share/man
mkdir -p /home/authdemodev/.local/share/NuGet
mkdir -p /home/authdemodev/.local/share/NuGet/http-cache

# Create the folders required for managing NuGet packages
mkdir -p /home/authdemodev/.nuget

# Create the folders required for managing project user secrets
mkdir -p /home/authdemodev/.microsoft
mkdir -p /home/authdemodev/.microsoft/usersecrets