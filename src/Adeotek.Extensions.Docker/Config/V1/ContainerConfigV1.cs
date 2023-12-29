using System.Text.Json.Serialization;

using YamlDotNet.Serialization;

namespace Adeotek.Extensions.Docker.Config.V1;

public class ContainerConfigV1
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
    public PortMappingV1[] Ports { get; set; } = Array.Empty<PortMappingV1>();
    // Volumes
    public VolumeConfigV1[] Volumes { get; set; } = Array.Empty<VolumeConfigV1>();
    // Environment variables
    public Dictionary<string, string> EnvVars { get; set; } = new();
    // Network
    public NetworkConfigV1? Network { get; set; }
    // Extra hosts entries
    public Dictionary<string, string> ExtraHosts { get; set; } = new();
    // Misc
    public string? Restart { get; set; }
    // Container startup
    public string? Command { get; set; }
    public string[] CommandArgs { get; set; } = Array.Empty<string>();
    public string[] RunCommandOptions { get; set; } = Array.Empty<string>();
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