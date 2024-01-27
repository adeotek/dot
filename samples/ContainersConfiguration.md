# Containers Configuration 
This page contains information about the `containers` command configuration files.
The `containers` command supports both YAML and JSON formats for the container configuration.

**New**: With the new `containers` command, the configuration file structure is mostly compatible with `docker-compose` configuration files.

**_Warning_**: there are a few options specific to this tool that are not used/recognize by `docker-compose`.

- YAML sample configuration: [sample-containers-config.yml](./sample-containers-config.yml)
- JSON sample configuration: [sample-containers-config.json](./sample-containers-config.json)

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

- **Services** (YAML|JSON key: `services`) [dictionary(serviceKey - ServiceConfig)] **required** : List of services (containers).
- **Networks** (YAML|JSON key: `networks`) [dictionary(networkKey - NetworkConfig)] *optional* : List of Docker networks.

### ServiceConfig

**(!)** Configuration options not supported by **Docker Compose**.

- **Image** (YAML|JSON key: `image`) [string] **required** : Container image (format: [&lt;registry&gt;/][&lt;project&gt;/]&lt;image&gt;[:&lt;tag&gt;|@&lt;digest&gt;]).
- **PullPolicy** (YAML key: `pull_policy` | JSON key: `pullPolicy`) [string] *optional* : Container image pull policy (`always`/`never`/`missing`, defaults to `missing` if NULL/omitted).
- (!) **ContainerName** (YAML key: `container_name` | JSON key: `containerName`) [string] *optional* * : Container full name (if missing, it will be generated from NamePrefix + BaseName + CurrentSuffix).
- (!) **NamePrefix** (YAML key: `name_prefix` | JSON key: `namePrefix`) [string] *optional* : Container name prefix.
- (!) **BaseName** (YAML key: `base_name` | JSON key: `baseName`) [string] *optional* * : Container base name.
- (!) **CurrentSuffix** (YAML key: `current_suffix` | JSON key: `CurrentSuffix`) [string] *optional* : Container name suffix for current version.
- (!) **PreviousSuffix** (YAML key: `previous_suffix` | JSON key: `PreviousSuffix`) [string] *optional* : Container name suffix for previous (backup/demoted) version.
- **Ports** (YAML|JSON key: `ports`) [array(PortMapping/string)] *optional* : The list of port mappings to be set for the container (see **PortMapping** below). If the string short syntax is used, the value will be converted to a **PortMapping** (short syntax: `[[<host>:]<published_port>:]<container_port>[/<protocol>]`).
- **Volumes** (YAML|JSON key: `volumes`) [array(VolumeConfig/string)] *optional* : The list of volumes to be mapped to the container (see **VolumeConfig** below). If the string short syntax is used, the value will be converted to a **VolumeConfig** (short syntax: `<volume_or_host_path>:<container_path>[:<access_mode>]`).
- **EnvFiles** (YAML key: `env_file` | JSON key: `envFiles`) [array(string)] *optional* : List of Environment Variables files.
- **EnvVars** (YAML key: `environment` | JSON key: `envVars`) [string] *optional* : [dictionary(Key-Value)] *optional* : The list of environment variable with their values to be set for the container.
  - `Key` [string] **required** : The name of the environment variable.
  - `Value` [string] **required** : The value of the environment variable.
- **Networks** (YAML|JSON key: `networks`) [dictionary(Key-Value)] *optional* : Container networks configuration.
  - `Key` [string] **required** : Network key (must exist in the **Networks** dictionary).
  - `Value` [ServiceNetworkConfig] **required** : Container configuration for a particular network (see **ServiceNetworkConfig** below).
- **Links** (YAML|JSON key: `links`) [array(string)] *optional* : Network linked services (Docker legacy services linking).
- **Hostname** (YAML|JSON key: `hostname`) [string] *optional* : Container hostname inside the virtual networks (defaults to **ContainerName**/**NamePrefix**+**BaseName**+**CurrentSuffix** if NULL/omitted or use an empty string for not setting a hostname).
- **ExtraHosts** (YAML key: `extra_hosts` | JSON key: `extraHosts`) [dictionary(Key-Value)] *optional* : A list of extra entries in the container's /etc/hosts.
  - `Key` [string] **required** : The key represents the host name.
  - `Value` [string] **required** : The value is the target IP address (this supports a special `host-gateway` value that resolves to the internal IP address of the host).
- **Dns** (YAML|JSON key: `dns`) [array(string)] *optional* : Custom DNS entries (if NULL, host's DNS will be used).
- **Expose** (YAML|JSON key: `expose`) [array(string)] *optional* : Defines the ports that the container exposes (these ports must be accessible to linked services and should not be published to the host machine, only the internal container ports can be specified).
- **Restart** (YAML|JSON key: `restart`) [string] *optional* : Container restart policy (`always`/`on-failure`/`unless-stopped`, defaults to `unless-stopped` if NULL/omitted).
- **Entrypoint** (YAML|JSON key: `entrypoint`) [string] *optional* : This overrides the *ENTRYPOINT* instruction from the service's Dockerfile.
- **Command** (YAML|JSON key: `command`) [array(string)] *optional* : Overrides the command declared by the container image, for example by Dockerfile's CMD.
- (!) **InitCliOptions** (YAML key: `init_cli_options` | JSON key: `initCliOptions`) [array(string)] *optional* : Docker/Podman `create`/`run` command options. (options supported by both Docker and Podman)
- (!) **DockerCliOptions** (YAML key: `docker_cli_options` | JSON key: `dockerCliOptions`) [array(string)] *optional* : Docker `create`/`run` specific options. (see [docker create](https://docs.docker.com/engine/reference/commandline/create/#options)/[docker run](https://docs.docker.com/engine/reference/commandline/run/#options))
- (!) **PodmanCliOptions** (YAML key: `podman_cli_options` | JSON key: `podmanCliOptions`) [array(string)] *optional* : Podman `create`/`run` specific options.

**&ast;** One of these configuration option is required. If **ContainerName** is provided, the **BaseName**, **NamePrefix**, **CurrentSuffix** and **PreviousSuffix**, will be ignored.

**PortMapping**:
- **Target** (YAML|JSON key: `target`) [string] **required** : Container port (internal port), can be a single port or a range of ports.
- **Published** (YAML|JSON key: `published`) [string] *optional* : Publicly exposed port, can be a single port or a range of ports. If NULL/omitted, the **Target** ports value will be used.
- **HostIp** (YAML key: `host_ip` | JSON key: `hostIp`) [string] *optional* : The Host IP mapping, unspecified means all network interfaces (0.0.0.0).
- **Protocol** (YAML|JSON key: `protocol`) [string] *optional* : The port protocol (`tcp` or `udp`), unspecified means any protocol.

**VolumeConfig**:
- **Type** (YAML|JSON key: `type`) [string] *optional* : The mount type (`volume`/`bind`/`tmpfs`/`npipe`/`cluster`, defaults to `volume` or `bind` if NULL/omitted, depending on the **Source** value).
- **Source** (YAML|JSON key: `source`) [string] **required** : The source path of the mount, a path on the host for a `bind` mount, or the name of a volume defined in the top-level volumes key. Not applicable for a `tmpfs` mount.
- **Target** (YAML|JSON key: `target`) [string] **required** : The path inside the container where the volume is mounted.
- **ReadOnly** (YAML key: `read_only` | JSON key: `readOnly`) [boolean] *optional* : Flag to set the volume as read-only.
- **Bind** (YAML|JSON key: `bind`) [VolumeBindConfig] *optional* : `bind` specific configuration, ignored for any other mount type:
  - **Propagation** (YAML|JSON key: `propagation`) [string] *optional* : The propagation mode used for the bind.
  - **CreateHostPath** (YAML key: `create_host_path` | JSON key: `createHostPath`) [boolean] *optional* : Creates a directory at the source path on host if there is nothing present. Does nothing if there is something present at the path.
  - **SeLinux** (YAML key: `selinux` | JSON key: `seLinux`) [string] *optional* : The SELinux re-labeling option z (shared) or Z (private)
- **Volume** (YAML|JSON key: `volume`) [VolumeVolumeConfig] *optional* : `volume` specific configuration, ignored for any other mount type:
  - **NoCopy** (YAML key: `nocopy` | JSON key: `noCopy`) [boolean] *optional* : Flag to disable copying of data from a container when a volume is created (defaults to FALSE if omitted).
- **TmpFs** (YAML key: `tmpfs` | JSON key: `tmpFs`) [VolumeTmpFsConfig] *optional* : `tmpfs` specific configuration, ignored for any other mount type:
  - **Size** (YAML|JSON key: `size`) [string] *optional* : The size for the tmpfs mount in bytes (either numeric or as bytes unit).
  - **Mode** (YAML|JSON key: `mode`) [string] *optional* : The file mode for the tmpfs mount as Unix permission bits as an octal number.
- (!) **SkipBackup** (YAML key: `skip_backup` | JSON key: `skipBackup`) [boolean] *optional* : If TRUE, the volume will not be backed up when running the backup command.

**ServiceNetworkConfig**:
- **IpV4Address** (YAML key: `ipv4_address` | JSON key: `ipV4Address`) [string] *optional* : Container's IP v4 address for the current network.
- **IpV6Address** (YAML key: `ipv6_address` | JSON key: `ipV6Address`) [string] *optional* : Container's IP v6 address for the current network.
- **Aliases** (YAML|JSON key: `aliases`) [array(string)] *optional* : Container's aliases in the current network.

### NetworkConfig

- **Name** (YAML|JSON key: `name`) [string] **required** : Docker network name.
- **Driver** (YAML|JSON key: `driver`) [string] **required** : Specifies which driver should be used for this network:
  - `host`: Use the host's networking stack.
  - `none`: Turn off networking.
  - `bridge`: Uses a software bridge which lets containers connected to the same bridge network communicate, while providing isolation from containers that aren't connected to that bridge network.
  - `overlay`: For communication among containers running on different Docker daemon hosts.
- **Ipam** (YAML|JSON key: `ipam`) [NetworkIpam] *optional* : Specifies a custom IPAM configuration:
  - **Driver** (YAML|JSON key: `driver`) [string] *optional* : IPAM driver (defaults to `default` if omitted).
  - **Config** (YAML|JSON key: `config`) [NetworkIpamConfig] **required** : IPAM config options. 
    - **Subnet** (YAML|JSON key: `subnet`) [string] **required** : Network subnet.
    - **IpRange** (YAML key: `ip_range` | JSON key: `ipRange`) [string] **required** : Network IP range (CIDR).
    - **Gateway** (YAML|JSON key: `gateway`) [string] *optional* : Network gateway address.
    - **AuxAddresses** (YAML key: `aux_addresses` | JSON key: `auxAddresses`) [dictionary(Key-Value)] *optional* : Auxiliary IPv4 or IPv6 addresses used by Network driver, as a mapping from hostname to IP.
      - `Key` [string] **required** : Hostname.
      - `Value` [string] **required** : IP address.
- **Attachable** (YAML|JSON key: `attachable`) [boolean] *optional* : If attachable is set to true, then standalone containers should be able to attach to this network, in addition to services. If a standalone container attaches to the network, it can communicate with services and other standalone containers that are also attached to the network. (defaults to TRUE if omitted)
- **External** (YAML|JSON key: `external`) [boolean] *optional* : Specifies that this network’s lifecycle is maintained outside of that of the application. We don't attempt to create these networks, and returns an error if one doesn't exist. (defaults to FALSE if omitted)
- **Internal** (YAML|JSON key: `internal`) [boolean] *optional* : By default, external connectivity to networks is provided. When **Internal** is set to TRUE, allows you to create an externally isolated network.
