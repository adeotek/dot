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
dot [options] <command>
```

Or

```shell
dotnet dot [options] <command>
```

### Commands

1. `container` - Docker containers management tool
   1. `up` - Create/Update Docker containers based on JSON/YAML configuration files
   2. `down` - Remove Docker containers based on JSON/YAML configuration files
   3. `config` - JSON/YAML configuration files generator/checker
      1. `validate` - validate existing JSON/YAML configuration file
      2. `sample` - generate new sample JSON/YAML configuration file
2. `utf8bom` - Add/Remove BOM (Byte Order Mark) from UTF-8 encoded files
   1. `add` - Add BOM (Byte Order Mark) to any UTF-8 files that do not have it
   2. `remove` - Remove BOM (Byte Order Mark) from any UTF-8 files that have it

Use `dot <command> --help` for more information about a command.
