ARG VARIANT=1.2.6-noble

# This is a base devcontainer image that runs Ubuntu 24.04 LTS with some basic tools for development.
FROM mcr.microsoft.com/devcontainers/base:${VARIANT}

ARG USERNAME=authdemodev
ARG USER_UID=1000
ARG USER_GID=${USER_UID}

# Create the user
RUN groupadd --gid ${USER_GID} ${USERNAME} &&\
    useradd --uid ${USER_UID} --gid ${USER_GID} --create-home --shell /bin/bash ${USERNAME} &&\
    echo ${USERNAME} ALL=\(root\) NOPASSWD:ALL > /etc/sudoers.d/${USERNAME} &&\
    chmod 0440 /etc/sudoers.d/${USERNAME}

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

# Switch to the non-root user
USER ${USERNAME}

# Add the.NET environment to the path
ENV PATH=$PATH:/home/${USERNAME}/.dotnet:/home/${USERNAME}/.dotnet/tools
