#!/bin/bash

# Setup the ASP.NET Code Generator tool
dotnet tool install --global dotnet-aspnet-codegenerator

# Setup the Entity Framework Core CLI tool
dotnet tool install --global dotnet-ef

# Install the Aspire Project Templates
dotnet new install Aspire.ProjectTemplates --force