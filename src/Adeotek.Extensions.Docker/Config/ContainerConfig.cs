using System.Text.Json.Serialization;

using YamlDotNet.Serialization;

namespace Adeotek.Extensions.Docker.Config;

public class ContainerConfig
{
    // Image
    public string Image { get; set; } = default!;
    public string? Tag { get; set; }
    // Container Name
    public string? NamePrefix { get; set; }
    public string BaseName { get; set; } = default!;
    public string? PrimarySuffix { get; set; }
    public string? BackupSuffix { get; set; }
    // Ports
    public PortMapping[] Ports { get; set; } = Array.Empty<PortMapping>();
    // Volumes
    public VolumeConfig[] Volumes { get; set; } = Array.Empty<VolumeConfig>();
    // Environment variables
    public Dictionary<string, string> EnvVars { get; set; } = new();
    // Network
    public NetworkConfig? Network { get; set; }
    // Misc
    public string? Restart { get; set; }
    // Computed
    [JsonIgnore] [YamlIgnore]
    public string ImageTag => string.IsNullOrEmpty(Tag) ? "latest" : Tag;
    [JsonIgnore] [YamlIgnore]
    public string FullImageName => $"{Image}:{ImageTag}";
    [JsonIgnore] [YamlIgnore]
    public string PrimaryName => $"{NamePrefix}{BaseName}{PrimarySuffix}";
    [JsonIgnore] [YamlIgnore]
    public string BackupName => $"{NamePrefix}{BaseName}{BackupSuffix}";
}