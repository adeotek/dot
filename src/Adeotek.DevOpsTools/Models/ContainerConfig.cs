using System.Text;

namespace Adeotek.DevOpsTools.Models;

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
    public string ImageTag => string.IsNullOrEmpty(Tag) ? "latest" : Tag;
    public string FullImageName => $"{Image}:{ImageTag}";
    public string PrimaryName => $"{NamePrefix}{BaseName}{PrimarySuffix}";
    public string BackupName => $"{NamePrefix}{BaseName}{BackupSuffix}";
}