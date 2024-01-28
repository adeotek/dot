using System.Text.Json.Serialization;

using YamlDotNet.Serialization;

namespace Adeotek.Extensions.Containers.Config;

public class ServiceNetworkConfig
{
    /// <summary>
    /// IPv4 address.
    /// </summary>
    [YamlMember(Alias = "ipv4_address")]
    public string? IpV4Address { get; set; }
    /// <summary>
    /// IPv6 address.
    /// </summary>
    [YamlMember(Alias = "ipv6_address")]
    public string? IpV6Address { get; set; }
    /// <summary>
    /// Virtual network aliases.
    /// </summary>
    [YamlMember(Alias = "aliases")]
    public string[]? Aliases { get; set; }
    
    // Computed
    [JsonIgnore] [YamlIgnore] public string NetworkName { get; private set; } = "N/A";
    public ServiceNetworkConfig SetNetworkName(string networkName)
    {
        NetworkName = networkName;
        return this;
    }
}