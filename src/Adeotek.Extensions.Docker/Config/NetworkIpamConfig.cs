using YamlDotNet.Serialization;

namespace Adeotek.Extensions.Docker.Config;

public class NetworkIpamConfig
{
    /// <summary>
    /// Network subnet.
    /// </summary>
    [YamlMember(Alias = "subnet")]
    public string Subnet { get; set; } = default!;
    /// <summary>
    /// IP range (CIDR).
    /// </summary>
    [YamlMember(Alias = "ip_range")]
    public string IpRange { get; set; } = default!;
    /// <summary>
    /// Gateway address.
    /// </summary>
    [YamlMember(Alias = "gateway")]
    public string? Gateway { get; set; }
    /// <summary>
    /// Auxiliary IPv4 or IPv6 addresses used by Network driver, as a mapping from hostname to IP.
    /// </summary>
    [YamlMember(Alias = "aux_addresses")]
    public Dictionary<string, string>? AuxAddresses { get; set; }
}