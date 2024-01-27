using YamlDotNet.Serialization;

namespace Adeotek.Extensions.Containers.Config;

public class NetworkIpam
{
    /// <summary>
    /// IPAM driver.
    /// </summary>
    [YamlMember(Alias = "driver")]
    public string Driver { get; set; } = "default";
    /// <summary>
    /// IPAM config options.
    /// </summary>
    [YamlMember(Alias = "config")]
    public NetworkIpamConfig Config { get; set; } = default!;
}