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

### `containers`

Docker containers management tool. The subcommand work in a similar way to `docker-compose`, but with the ability to target individual services (containers).
The subcommands allows for containers manipulation based on configuration files written in YAML or JSON.
The new `containers` command replaces the old `container` one (v1).

**New**: The new `containers` command configuration file structure is mostly compatible with `docker-compose` configuration files (see [ContainersConfiguration.md](./samples/ContainersConfiguration.md)).

**_Warning_**: The old `container` command can still be used together with the old configuration format, but it is deprecated and will be removed in future versions.  

#### Subcommands

- `up` - Create/Update Docker containers based on YAML/JSON configuration files
- `down` - Remove Docker containers based on YAML/JSON configuration files
- `backup` - Backup Docker containers volumes based on YAML/JSON configuration files
- `start` - Start Docker containers based on YAML/JSON configuration files
- `stop` - Stop Docker containers based on YAML/JSON configuration files
- `restart` - Restart Docker containers based on YAML/JSON configuration files
- `config` - YAML/JSON configuration files generator/checker
   - `validate` - Validate existing YAML/JSON configuration file
   - `sample` - Generate new sample YAML/JSON configuration file

Check [ContainersConfiguration.md](./samples/ContainersConfiguration.md) for more information about the configuration files.

### `email`

Email tools.

#### Subcommands

- `send` - Send an email message based on a configuration file or provided options

Check [EmailConfiguration.md](./samples/EmailConfiguration.md) for more information about the configuration files.

### `port`

TCP Ports tools.

#### Subcommands

- `listen` - Start a listener on the provided TCP port
- `probe` - Probe (check if listening) a local or remote TCP port

### `utf8bom`

This tool allows for adding/removing/checking the BOM (Byte Order Mark) signature of UTF-8 encoded files.

#### Subcommands

- `add` - Add BOM (Byte Order Mark) to any UTF-8 files that do not have it
- `remove` - Remove BOM (Byte Order Mark) from any UTF-8 files that have it

To only check if BOM is present, without changing it, use the `add`/`remove` subcommands together with the `--dry-run` option. 
