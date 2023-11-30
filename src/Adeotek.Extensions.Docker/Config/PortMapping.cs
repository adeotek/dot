using YamlDotNet.Serialization;

namespace Adeotek.Extensions.Docker.Config;

/// <summary>
/// Container port mapping.
/// Short syntax: [HOST:]CONTAINER[/PROTOCOL]
/// - HOST is [IP:](port | range)
/// - CONTAINER is port | range
/// - PROTOCOL to restrict port to specified protocol (tcp or udp).
/// </summary>
public class PortMapping
{
    /// <summary>
    /// The container port.
    /// </summary>
    [YamlMember(Alias = "target")]
    public uint Target { get; set; }
    /// <summary>
    /// The publicly exposed port.
    /// It is defined as a string and can be set as a range using syntax start-end.
    /// It means the actual port is assigned a remaining available port, within the set range.
    /// </summary>
    [YamlMember(Alias = "published")]
    public string? Published { get; set; }
    /// <summary>
    /// The Host IP mapping, unspecified means all network interfaces (0.0.0.0).
    /// </summary>
    [YamlMember(Alias = "host_ip")]
    public string? HostIp { get; set; }
    /// <summary>
    /// The port protocol (`tcp` or `udp`), unspecified means any protocol.
    /// </summary>
    [YamlMember(Alias = "protocol")]
    public string? Protocol { get; set; }
    /// <summary>
    /// Use `host` for publishing a host port on each node, or `ingress` for a port to be load balanced.
    /// </summary>
    [YamlMember(Alias = "mode")]
    public string? Mode { get; set; }
}