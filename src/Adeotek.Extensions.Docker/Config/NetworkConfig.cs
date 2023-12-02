using System.Text.Json.Serialization;

using YamlDotNet.Serialization;

namespace Adeotek.Extensions.Docker.Config;

public class NetworkConfig
{
    /// <summary>
    /// Network name.
    /// </summary>
    [YamlMember(Alias="name")]
    public string Name { get; set; } = default!;
    /// <summary>
    /// Specifies which driver should be used for this network.
    /// - host: Use the host's networking stack.
    /// - none: Turn off networking.
    /// - bridge: Uses a software bridge which lets containers connected to the same bridge network communicate,
    ///           while providing isolation from containers that aren't connected to that bridge network.
    /// - overlay: For communication among containers running on different Docker daemon hosts.
    /// </summary>
    [YamlMember(Alias="driver")]
    public string Driver { get; set; } = default!;
    /// <summary>
    /// Specifies a custom IPAM configuration.
    /// </summary>
    [YamlMember(Alias="ipam")]
    public NetworkIpam? Ipam { get; set; }
    /// <summary>
    /// If attachable is set to true, then standalone containers should be able to attach to this network,
    /// in addition to services. If a standalone container attaches to the network,
    /// it can communicate with services and other standalone containers that are also attached to the network.
    /// </summary>
    [YamlMember(Alias="attachable")]
    public bool Attachable { get; set; } = true;
    /// <summary>
    /// Specifies that this network’s lifecycle is maintained outside of that of the application.
    /// We don't attempt to create these networks, and returns an error if one doesn't exist.
    /// </summary>
    [YamlMember(Alias="external")]
    public bool External { get; set; }
    /// <summary>
    /// By default, external connectivity to networks is provided.
    /// `internal`, when set to true, allows you to create an externally isolated network.
    /// </summary>
    [YamlMember(Alias="internal")]
    public bool Internal { get; set; }
    /// <summary>
    /// If TRUE, the network will be preserved on purge, otherwise it will be deleted.
    /// [NOT SUPPORTED by Docker Compose]
    /// </summary>
    [YamlMember(Alias="preserve")]
    public bool Preserve { get; set; }
    
    // Computed
    [JsonIgnore] [YamlIgnore] public string NetworkName { get; private set; } = "N/A";
    public NetworkConfig SetNetworkName(string networkName)
    {
        NetworkName = networkName;
        return this;
    }
}