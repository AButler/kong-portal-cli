# Kong Portal CLI

[![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/AButler/kong-portal-cli/main.yml)](https://github.com/AButler/kong-portal-cli/actions/workflows/main.yml)
[![GitHub release (latest SemVer including pre-releases)](https://img.shields.io/github/v/release/AButler/kong-portal-cli?include_prereleases)](https://github.com/AButler/kong-portal-cli/releases)
[![GitHub](https://img.shields.io/github/license/AButler/kong-portal-cli)](https://github.com/AButler/kong-portal-cli/blob/main/LICENSE)

> 🚧 Note: this tool is currently under development
 
A command line tool for dumping API Products and documentation from Kong Konnect and syncing them back.

## Overview

- [Features](#features)
- [Installation](#installation)
- [Documentation](#documentation)

## Features

- **Dump**  
  Dumps Kong Konnect API Products, API Product Versions, API Product Documentation and Portal 
  settings to disk for storing within a source control repository 
- **Sync**  
  Synchronizes API Products, API Product Versions, API Product Documentation and Portal
  settings stored on disk to Kong Konnect

## Installation

### Linux

Download the binary from the [release page](https://github.com/AButler/kong-portal-cli/releases)

```bash
# Using bash
export PORTAL_CLI_VERSION=0.0.1-alpha003
curl -sL "https://github.com/AButler/kong-portal-cli/releases/download/v${PORTAL_CLI_VERSION}/portal-cli-linux-x64.tar.gz" -o /tmp/portal-cli.tar.gz
tar -zxf /tmp/portal-cli.tar.gz -C /tmp
sudo cp /tmp/portal-cli /usr/local/bin/
```

### Windows

Download the binary from the [release page](https://github.com/AButler/kong-portal-cli/releases)

```powershell
# Using powershell
$PORTAL_CLI_VERSION="0.0.1-alpha003"
curl -sL "https://github.com/AButler/kong-portal-cli/releases/download/v$PORTAL_CLI_VERSION/portal-cli-win-x64.zip" -o $env:TEMP/portal-cli.zip
Expand-Archive $env:TEMP/portal-cli.zip .
```

> 💡 Tip: Move `portal-cli.exe` to a location that is within your `PATH` environment variable

### Build your own Docker image

Use this snippet in your own `Dockerfile` to download

```dockerfile
ARG PORTAL_CLI_VERSION=0.0.1-alpha003

RUN export CONTAINER_ARCH="$(uname -m)" \
    export MUSL="$(ldd /bin/ls | grep 'musl' | head -1 | cut -d ' ' -f1)" \
    && if [[ ${CONTAINER_ARCH} == "x86_64" ]]; then if [[ -n ${MUSL} ]]; then export ARCH="musl-x64"; else export ARCH="x64"; fi; elif [[ ${CONTAINER_ARCH} == "aarch64" ]]; then export ARCH="arm64"; fi \
    && curl -L "https://github.com/AButler/kong-portal-cli/releases/download/v${PORTAL_CLI_VERSION}/portal-cli-linux-${ARCH}.tar.gz" -o /tmp/portal-cli.tar.gz \
    && tar zxf /tmp/portal-cli.tar.gz -C /usr/bin portal-cli \
    && chmod +x /usr/bin/portal-cli
```

## Documentation

You can use `--help` flag once you've got the Portal CLI installed on your system to get help in the terminal itself.

```bash
portal-cli --help
```
