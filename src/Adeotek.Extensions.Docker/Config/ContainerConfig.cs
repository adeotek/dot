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
    public string Name { get; set; } = default!;
    public string? CurrentSuffix { get; set; }
    public string? PreviousSuffix { get; set; }
    // Ports
    public PortMapping[] Ports { get; set; } = Array.Empty<PortMapping>();
    // Volumes
    public VolumeConfig[] Volumes { get; set; } = Array.Empty<VolumeConfig>();
    // Environment variables
    public Dictionary<string, string> EnvVars { get; set; } = new();
    // Network
    public NetworkConfig? Network { get; set; }
    // Extra hosts entries
    public Dictionary<string, string> ExtraHosts { get; set; } = new();
    // Misc
    public string? Restart { get; set; }
    // Computed
    [JsonIgnore] [YamlIgnore]
    public string ImageTag => string.IsNullOrEmpty(Tag) ? "latest" : Tag;
    [JsonIgnore] [YamlIgnore]
    public string FullImageName => $"{Image}:{ImageTag}";
    [JsonIgnore] [YamlIgnore]
    public string CurrentName => $"{NamePrefix}{Name}{CurrentSuffix}";
    [JsonIgnore] [YamlIgnore]
    public string PreviousName => $"{NamePrefix}{Name}{PreviousSuffix}";
}