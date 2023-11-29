namespace Adeotek.Extensions.Docker.Config;

public class NetworkBridgeConfig
{
    public string Subnet { get; set; } = default!;
    public string IpRange { get; set; } = default!;
    public string? Gateway { get; set; }
}