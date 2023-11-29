namespace Adeotek.Extensions.Docker.Config;

public class NetworkConfig
{
    /// <summary>
    /// Network name.
    /// </summary>
    public string Name { get; set; } = default!;
    /// <summary>
    /// Specifies which driver should be used for this network.
    /// - host: Use the host's networking stack.
    /// - none: Turn off networking.
    /// - bridge: Uses a software bridge which lets containers connected to the same bridge network communicate,
    ///           while providing isolation from containers that aren't connected to that bridge network.
    /// - overlay: For communication among containers running on different Docker daemon hosts.
    /// </summary>
    public string Driver { get; set; } = default!;
    /// <summary>
    /// Bridge network specific configuration (optional).
    /// </summary>
    public NetworkBridgeConfig? BridgeConfig { get; set; }
    /// <summary>
    /// If attachable is set to true, then standalone containers should be able to attach to this network,
    /// in addition to services. If a standalone container attaches to the network,
    /// it can communicate with services and other standalone containers that are also attached to the network.
    /// </summary>
    public bool Attachable { get; set; } = true;
    /// <summary>
    /// Specifies that this network’s lifecycle is maintained outside of that of the application.
    /// We don't attempt to create these networks, and returns an error if one doesn't exist.
    /// </summary>
    public bool External { get; set; }
    /// <summary>
    /// By default, external connectivity to networks is provided.
    /// `internal`, when set to true, allows you to create an externally isolated network.
    /// </summary>
    public bool Internal { get; set; }
}