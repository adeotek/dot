# dot - AdeoTEK DevOps Tools

`dot` is a collection of CLI tools for developers and DevOps.

[![.NET build](https://github.com/adeotek/dot/actions/workflows/dotnet_build.yml/badge.svg)](https://github.com/adeotek/dot/actions/workflows/dotnet_build.yml)

## Install

```shell
dotnet tool install -g AdeoTEK.DevOps.Tools
```

## Update

```shell
dotnet tool update -g AdeoTEK.DevOps.Tools
```

AdeoTEK DevOps Tools NuGet package can be found [here](https://www.nuget.org/packages/AdeoTEK.DevOps.Tools/).

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

Use `dot <command> --help` for more information about a command.
