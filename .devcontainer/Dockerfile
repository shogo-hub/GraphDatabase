FROM ubuntu:22.04

ARG DEBIAN_FRONTEND=noninteractive

# Base tooling + C++ deps commonly needed for Faiss
RUN apt-get update && apt-get install -y --no-install-recommends \
    ca-certificates \
    curl \
    git \
    sudo \
    wget \
    build-essential \
    cmake \
    ninja-build \
    pkg-config \
    cppcheck \
    valgrind \
    gdb \
    libopenblas-dev \
    liblapack-dev \
    libomp-dev \
    && rm -rf /var/lib/apt/lists/*

# Install .NET 9 SDK + ASP.NET runtime (matches net9.0)
RUN wget -q https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O /tmp/packages-microsoft-prod.deb \
    && dpkg -i /tmp/packages-microsoft-prod.deb \
    && rm /tmp/packages-microsoft-prod.deb \
    && apt-get update \
    && apt-get install -y --no-install-recommends dotnet-sdk-9.0 aspnetcore-runtime-9.0 \
    && rm -rf /var/lib/apt/lists/*

# Create a non-root user for VS Code
ARG USERNAME=vscode
ARG USER_UID=1000
ARG USER_GID=1000
RUN groupadd --gid ${USER_GID} ${USERNAME} \
    && useradd -s /bin/bash --uid ${USER_UID} --gid ${USER_GID} -m ${USERNAME} \
    && echo "${USERNAME} ALL=(ALL) NOPASSWD:ALL" >> /etc/sudoers

USER ${USERNAME}
WORKDIR /workspaces/GraphDatabase