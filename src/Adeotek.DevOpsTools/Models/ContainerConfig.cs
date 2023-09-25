using System.Text;

namespace Adeotek.DevOpsTools.Models;

public class ContainerConfig
{
    // Image
    public string Image { get; set; } = default!;
    public string? ImageTag { get; set; }
    // Container Name
    public string BaseName { get; set; } = default!;
    public string? NamePrefix { get; set; }
    public string? PrimarySuffix { get; set; }
    public string? BackupSuffix { get; set; }
    // Ports
    public PortMapping[] Ports { get; set; } = Array.Empty<PortMapping>();
    // Volumes
    public VolumeConfig[] Volumes { get; set; } = Array.Empty<VolumeConfig>();
    // Environment variables
    public Dictionary<string, string> EnvVars { get; set; } = new();
    // Network
    public string? NetworkName { get; set; }
    public string? NetworkSubnet { get; set; }
    public string? NetworkIpRange { get; set; }
    // public NetworkConfig? Network { get; set; }
    public string? Hostname { get; set; }
    public string? NetworkAlias { get; set; }
    // Misc
    public string? Restart { get; set; }
    // Computed
    public string PrimaryName => $"{NamePrefix}{BaseName}{PrimarySuffix}";
    public string BackupName => $"{NamePrefix}{BaseName}{BackupSuffix}";
}