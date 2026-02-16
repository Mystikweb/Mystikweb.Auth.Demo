ARG VARIANT=noble

# This is a base devcontainer image that runs Ubuntu 24.04 LTS with some basic tools for development.
FROM mcr.microsoft.com/devcontainers/base:${VARIANT}

# Install additional OS updates and packages.
RUN apt-get update && \
    export DEBIAN_FRONTEND=noninteractive && \
    apt-get -y upgrade && \
    apt-get -y install --no-install-recommends software-properties-common && \
    apt-get clean -y && \
    rm -rf /var/lib/apt/lists/*

# Add workspace build scripts that can be run after the base image OS updates.
COPY ./buildWorkspace.sh buildWorkspace.sh

# Run the workspace build script to generate the required folder structure
# as the root user during the image build process.
RUN bash ./buildWorkspace.sh

# Add the.NET environment to the path
ENV PATH=$PATH:/home/vscode/.dotnet:/home/vscode/.dotnet/tools
