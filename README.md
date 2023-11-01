# dot - AdeoTEK DevOps Tools

`dot` is a collection of CLI tools for developers and DevOps.

[![.NET build](https://github.com/adeotek/dot/actions/workflows/dotnet_build.yml/badge.svg)](https://github.com/adeotek/dot/actions/workflows/dotnet_build.yml)

## Install

AdeoTEK DevOps Tools NuGet package can be found [on nuget.org](https://www.nuget.org/packages/AdeoTEK.DevOpsTools/).

```shell
# Install latest version (preview version are not included)
dotnet tool install --global AdeoTEK.DevOpsTools

# Install specific version
dotnet tool install --global Adeotek.DevOpsTools --version <specific_version>
```

## Update

```shell
dotnet tool update -g AdeoTEK.DevOpsTools
```

## Usage

```shell
dot <command> <subcommand> [arguments] [options]
# or
dotnet dot <command> <subcommand> [arguments] [options]
```

To get more information about a command, run:

```shell
dot <command> --help
# or
dot <command> <subcommand> --help
```

## Commands

### `container`

Docker containers management tool. The subcommand work in a similar way to `docker-compose`, but referring only to individual containers.
The subcommands allows for the creation/update/removal of containers, based on configuration files written in YAML or JSON.  

#### Subcommands

- `up` - Create/Update Docker containers based on YAML/JSON configuration files
- `down` - Remove Docker containers based on YAML/JSON configuration files
- `config` - YAML/JSON configuration files generator/checker
   - `validate` - Validate existing YAML/JSON configuration file
   - `sample` - Generate new sample YAML/JSON configuration file

Check [ContainerConfiguration.md](./samples/ContainerConfiguration.md) for more information about the configuration files.

### `utf8bom`

This tool allows for adding/removing/checking the BOM (Byte Order Mark) signature of UTF-8 encoded files.

#### Subcommands

- `add` - Add BOM (Byte Order Mark) to any UTF-8 files that do not have it
- `remove` - Remove BOM (Byte Order Mark) from any UTF-8 files that have it

In order to only check the BOM existence, without changing it, use the `add`/`remove` subcommands together with the `--dry-run` option. 

