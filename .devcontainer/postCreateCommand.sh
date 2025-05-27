#!/bin/bash

# Setup the Entity Framework Core CLI tool
dotnet tool install --global dotnet-ef

# Install the Aspire Project Templates
dotnet new install Aspire.ProjectTemplates --force