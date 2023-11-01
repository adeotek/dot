# Container Configuration 
This page contains information about the `container` command configuration files.
The `container` command supports both YAML and JSON formats for the container configuration.

- YAML sample configuration: [sample-config.yml](./sample-config.yml)
- JSON sample configuration: [sample-config.json](./sample-config.json)

The `config sample` subcommand can be used to generate a sample configuration into a file or as output into the console.

```shell
# Generate YAML sample to console
dot container config sample screen

# Generate YAML sample file
dot container config sample /path/to/target_file.yml

# Generate JSON sample file
dot container config sample /path/to/target_file.json --format json
```

## Configuration file structure

- `Image` [string] **required** : Docker image. (without a tag)
- `Tag` [string] *optional* : Docker image tag. (defaults to `latest` if null/omitted)
- `NamePrefix` [string] *optional* : Container (both primary and demoted) name prefix. (will not be used if null/omitted)
- `Name` [string] *optional* : Container name.
- `CurrentSuffix` [string] *optional* : Current container name suffix. (will not be used if null/missing)
- `PreviousSuffix` [string] *optional* : Previous (demoted) container name suffix. (will not be used if null/omitted)
- `Ports` [array(PortMapping)] *optional* : The list of port mappings to be set for the container. (see **PortMapping** below)
- `Volumes` [array(Volume)] *optional* : The list of volumes to be set for the container. (see **Volume** below)
- `EnvVars` [dictionary(key-value)] *optional* : The list of environment variable with their values to be set for the container.
  - `Key` [string] **required** : The name of the environment variable.
  - `Value` [string] **required** : The value of the environment variable.
- `Network` [object(NetworkConfig)] *optional* : Container network configuration. (see **NetworkConfig** below)
- `ExtraHosts` [dictionary(key-value)] *optional* : A list of extra entries in the container's /etc/hosts.
  - `Key` [string] **required** : The key represents the host name.
  - `Value` [string] **required** : The value is the target IP address (this supports a special `host-gateway` value that resolves to the internal IP address of the host).
- `Restart` [string] *optional* : Container restart policy. The allowed values are: `always`, `on-failure` and `unless-stopped` (defaults to `unless-stopped` if null/omitted)

**PortMapping**:
- `Host` [unsigned int] **required** : Port number on the host.
- `Container` [unsigned int] **required** : Port number in the container.

**Volume**:
- `Source` [string] **required** : Docker volume name if `IsBind` is `false`, or a path on the host `IsBind` is `true`. 
- `Destination` [string] **required** : Path inside the container. 
- `IsBind` [boolean] *optional* : Indicates if a path on the host will be used instead of a Docker volume (bind volume). (defaults to `true` if omitted)
- `AutoCreate` [boolean] *optional* : If `true` the Docker Volume or the path on the host will be automatically created, if missing. (defaults to `true` if omitted)

**NetworkConfig**:
- `Name` [string] *optional* : Docker network name.
- `Subnet` [string] *optional*(1) : Docker network subnet (i.e. 172.1.0.1/24).
- `IpRange` [string] *optional*(1) : Docker network IP addresses range (i.e. 172.1.0.1/26).
- `IpAddress` [string] *optional* : Container static IP address. 
- `Hostname` [string] *optional* : Container network hostname. `NamePrefix`+`BaseName`+`PrimarySuffix` will be used if null/omitted. Use an empty string for not setting any hostname.
- `Alias` [string] *optional* : Container network (hostname) alias. `BaseName` will be used if null/omitted. Use an empty string for not setting any alias.
- `IsShared` [boolean] *optional* : Indicates if the Docker network is used by other containers. If set to `true`, the network will not be removed when running the `down` subcommand with the `--purge` option. (defaults to `false` if omitted)

**(1)** - *The field is* **required** *if the network `Name` is provided and the network should be created together with the container, when it doesn't exist*. Will be ignored if `Name` is null or empty.
