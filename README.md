# Kong Portal CLI

[![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/AButler/kong-portal-cli/main.yml)](https://github.com/AButler/kong-portal-cli/actions/workflows/main.yml)
[![GitHub release (latest SemVer)](https://img.shields.io/github/v/release/AButler/kong-portal-cli)](https://github.com/AButler/kong-portal-cli/releases)
[![GitHub](https://img.shields.io/github/license/AButler/kong-portal-cli)](https://github.com/AButler/kong-portal-cli/blob/main/LICENSE)
 
A command line tool for dumping API Products and documentation from Kong Konnect and syncing them back.

## Overview

- [Features](#features)
- [Installation](#installation)
- [Documentation](#documentation)
- [Supported Entities](#supported-entities)

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
export PORTAL_CLI_VERSION=1.0.0
curl -sL "https://github.com/AButler/kong-portal-cli/releases/download/v${PORTAL_CLI_VERSION}/portal-cli-linux-x64.tar.gz" -o /tmp/portal-cli.tar.gz
tar -zxf /tmp/portal-cli.tar.gz -C /tmp
sudo cp /tmp/portal-cli /usr/local/bin/
```

### Windows

Download the binary from the [release page](https://github.com/AButler/kong-portal-cli/releases)

```powershell
# Using powershell
$PORTAL_CLI_VERSION="1.0.0"
curl -sL "https://github.com/AButler/kong-portal-cli/releases/download/v$PORTAL_CLI_VERSION/portal-cli-win-x64.zip" -o $env:TEMP/portal-cli.zip
Expand-Archive $env:TEMP/portal-cli.zip .
```

> 💡 Tip: Move `portal-cli.exe` to a location that is within your `PATH` environment variable

### Build your own Docker image

Use this snippet in your own `Dockerfile` to download

```dockerfile
ARG PORTAL_CLI_VERSION=1.0.0

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

## Variables

You can use variables within the json files for defining entities when performing a sync.

### Environment Variables

Use the syntax `${{ env ENVIRONMENT_VARIABLE_NAME }}` to replace with an environment variable.

### Argument Variables

Use the syntax `${{ var VARIABLE_NAME }}` to replace with a variable passed on the command line.

These can be entered on the command line using the following syntax:

```bash
portal-cli sync --var VARIABLE_NAME=variable_value
```

> Note: to include spaces in the value, surround both the key and value with quotes (`"`), e.g. `"VARIABLE_NAME=variable value"`

### Example

Environment Variables:

`COMPANY_NAME=Acme`

Argument Variables:

```bash
portal-cli sync --var VERSION=1.0 --var "VERSION_NAME=Interesting Otter"
```

Sample JSON:

```json
{
  "sync_id": "sample-api",
  "name": "Sample API",
  "description": "${{ env COMPANY_NAME }} Sample API. Version ${{ var VERSION }} - ${{ var VERSION_NAME }}",
  "portals": [
    "default"
  ],
  "labels": {}
}
```

Transformed JSON:
```json
{
  "sync_id": "sample-api",
  "name": "Sample API",
  "description": "Acme Sample API. Version 1.0 - Interesting Otter",
  "portals": [
    "default"
  ],
  "labels": {}
}
```
## Supported Entities

The table below shows the list of Kong Konnect entities that are supported by the sync.

| Entity                            | Supported | Notes                           |
|-----------------------------------|-----------|---------------------------------|
| API Product                       | ✅         |                                 |
| API Product Document              | ✅         |                                 |
| API Product Version               | ✅         |                                 |
| API Product Version Specification | ✅         |                                 |
| Portal                            | ✅*        | Cannot create or delete Portals |
| Portal Appearance                 | ✅         |                                 |
| Portal Auth Settings              | ✅         |                                 |
| Portal Teams                      | ✅         |                                 |
| Portal Team Group Mappings        | ❌         |                                 |
| Portal Team Roles                 | ❌         |                                 |

