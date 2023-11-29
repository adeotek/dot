namespace Adeotek.Extensions.Docker.Config;

public class PortMapping
{
    /// <summary>
    /// The container port.
    /// </summary>
    public uint Target { get; set; }
    /// <summary>
    /// The publicly exposed port.
    /// It is defined as a string and can be set as a range using syntax start-end.
    /// It means the actual port is assigned a remaining available port, within the set range.
    /// </summary>
    public string? Published { get; set; }
    /// <summary>
    /// The Host IP mapping, unspecified means all network interfaces (0.0.0.0).
    /// </summary>
    public string HostIp { get; set; } = "0.0.0.0";
    /// <summary>
    /// The port protocol (`tcp` or `udp`), unspecified means any protocol.
    /// </summary>
    public string Protocol { get; set; } = "tcp";
    /// <summary>
    /// Use `host` for publishing a host port on each node, or `ingress` for a port to be load balanced.
    /// </summary>
    public string Mode { get; set; } = "host";
}