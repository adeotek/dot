﻿using System.Text.Json.Serialization;

using Adeotek.Extensions.ConfigFiles;

using YamlDotNet.Serialization;

namespace Adeotek.Extensions.Containers.Config;

public class ServiceConfig
{
    public static readonly string[] PullPolicyValues = { "always", "never", "missing" };
    public static readonly string[] RestartPolicyValues = { "no", "always", "on-failure", "unless-stopped" };
    
    /// <summary>
    /// Container image.
    /// Format: [&lt;registry&gt;/][&lt;project&gt;/]&lt;image&gt;[:&lt;tag&gt;|@&lt;digest&gt;]
    /// </summary>
    [YamlMember(Alias = "image")]
    public string Image { get; set; } = default!;
    /// <summary>
    /// Image pull policy.
    /// Allowed values: always/never/missing
    /// </summary>
    [YamlMember(Alias = "pull_policy")]
    public string? PullPolicy { get; set; }
    /// <summary>
    /// Container full name.
    /// If missing, it will be generated from NamePrefix + BaseName + CurrentSuffix.
    /// </summary>
    [YamlMember(Alias = "container_name")]
    public string? ContainerName { get; set; }
    /// <summary>
    /// Container name prefix.
    /// [NOT SUPPORTED by Docker Compose]
    /// </summary>
    [YamlMember(Alias = "name_prefix")]
    public string? NamePrefix { get; set; }
    /// <summary>
    /// Container base name.
    /// [NOT SUPPORTED by Docker Compose]
    /// </summary>
    [YamlMember(Alias = "base_name")]
    public string? BaseName { get; set; }
    /// <summary>
    /// Container name suffix for current version.
    /// [NOT SUPPORTED by Docker Compose]
    /// </summary>
    [YamlMember(Alias = "current_suffix")]
    public string? CurrentSuffix { get; set; }
    /// <summary>
    /// Container name suffix for previous (backup) version.
    /// [NOT SUPPORTED by Docker Compose]
    /// </summary>
    [YamlMember(Alias = "previous_suffix")]
    public string? PreviousSuffix { get; set; }
    /// <summary>
    /// Sets the container to run with elevated privileges.
    /// </summary>
    [YamlMember(Alias = "privileged")]
    public bool Privileged { get; set; }
    /// <summary>
    /// Container's ports to expose.
    /// </summary>
    [YamlMember(Alias = "ports")]
    public PortMapping[]? Ports { get; set; }
    /// <summary>
    /// Container's attached volumes.
    /// </summary>
    [YamlMember(Alias = "volumes")]
    public VolumeConfig[]? Volumes { get; set; }
    /// <summary>
    /// Environment variables loaded from one or more files.
    /// </summary>
    [YamlMember(Alias = "env_file")]
    public StringArray? EnvFiles { get; set; }
    /// <summary>
    /// Environment variables.
    /// </summary>
    [YamlMember(Alias = "environment")]
    public Dictionary<string, string>? EnvVars { get; set; }
    /// <summary>
    /// Attached virtual networks.
    /// </summary>
    [YamlMember(Alias = "networks")]
    public Dictionary<string, ServiceNetworkConfig?>? Networks { get; set; }
    /// <summary>
    /// Network linked services.
    /// </summary>
    [YamlMember(Alias = "links")]
    public StringArray? Links { get; set; }
    /// <summary>
    /// Container hostname inside the virtual networks.
    /// </summary>
    [YamlMember(Alias = "hostname")]
    public string? Hostname { get; set; }
    /// <summary>
    /// Extra hosts entries.
    /// </summary>
    [YamlMember(Alias = "extra_hosts")]
    public Dictionary<string, string>? ExtraHosts { get; set; }
    /// <summary>
    /// Custom DNS entries. If NULL, host's DNS will be used.
    /// </summary>
    [YamlMember(Alias = "dns")]
    public StringArray? Dns { get; set; }
    /// <summary>
    /// Defines the ports that the container exposes.
    /// These ports must be accessible to linked services and should not be published to the host machine.
    /// Only the internal container ports can be specified.
    /// </summary>
    [YamlMember(Alias = "expose")]
    public StringArray? Expose { get; set; }
    /// <summary>
    /// Container restart policy.
    /// </summary>
    [YamlMember(Alias = "restart")]
    public string? Restart { get; set; }
    /// <summary>
    /// Set the HEALTHCHECK parameters for the container.
    /// </summary>
    [YamlMember(Alias = "healthcheck")]
    public ServiceHealthCheckConfig? HealthCheck { get; set; }
    /// <summary>
    /// This overrides the ENTRYPOINT instruction from the service's Dockerfile.
    /// </summary>
    [YamlMember(Alias = "entrypoint")]
    public string? Entrypoint { get; set; }
    /// <summary>
    /// Overrides the default command declared by the container image, for example by Dockerfile's CMD.
    /// </summary>
    [YamlMember(Alias = "command")]
    public StringArray? Command { get; set; }
    /// <summary>
    /// Container labels.
    /// </summary>
    [YamlMember(Alias = "labels")]
    public Dictionary<string, string>? Labels { get; set; }
    /// <summary>
    /// overrides the user used to run the container process.
    /// The default is set by the image (i.e. Dockerfile USER). If it's not set, then root.
    /// </summary>
    [YamlMember(Alias = "user")]
    public string? User { get; set; }
    /// <summary>
    /// List of services required by the current service.
    /// </summary>
    [YamlMember(Alias = "depends_on")]
    public StringArray? DependsOn { get; set; }
    /// <summary>
    /// Docker/Podman `create`/`run` command options.
    /// [NOT SUPPORTED by Docker Compose]
    /// </summary>
    [YamlMember(Alias = "init_cli_options")]
    public StringArray? InitCliOptions { get; set; }
    /// <summary>
    /// Docker `create`/`run` command options.
    /// [NOT SUPPORTED by Docker Compose]
    /// </summary>
    [YamlMember(Alias = "docker_cli_options")]
    public StringArray? DockerCliOptions { get; set; }
    /// <summary>
    /// Podman `create`/`run` specific options.
    /// [NOT SUPPORTED by Docker Compose]
    /// </summary>
    [YamlMember(Alias = "podman_cli_options")]
    public StringArray? PodmanCliOptions { get; set; }
    
    // Computed
    [JsonIgnore] [YamlIgnore]
    public string CurrentName => string.IsNullOrEmpty(ContainerName) 
        ? $"{NamePrefix}{BaseName}{CurrentSuffix}"
        : ContainerName;
    [JsonIgnore] [YamlIgnore]
    public string? PreviousName => string.IsNullOrEmpty(ContainerName) && !string.IsNullOrEmpty(PreviousSuffix) 
        ? $"{NamePrefix}{BaseName}{PreviousSuffix}"
        : null;
    [JsonIgnore] [YamlIgnore] public string ServiceName { get; private set; } = "N/A";
    public ServiceConfig SetServiceName(string serviceName)
    {
        ServiceName = serviceName;
        return this;
    }
}