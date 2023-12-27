namespace Adeotek.Extensions.Docker.Config.V1;

public class NetworkConfigV1
{
    public string Name { get; set; } = default!;
    public string? Subnet { get; set; }
    public string? IpRange { get; set; }
    public string? IpAddress { get; set; }
    public string? Hostname { get; set; }
    public string? Alias { get; set; }
    public bool IsShared { get; set; }
}