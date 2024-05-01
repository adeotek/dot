using YamlDotNet.Serialization;

namespace Adeotek.Extensions.Containers.Config;

public class ContainersConfig
{
    /// <summary>
    /// Version.
    /// </summary>
    [YamlMember(Alias = "version")]
    public string? Version { get; set; }
    /// <summary>
    /// Services dictionary.
    /// </summary>
    [YamlMember(Alias = "services")]
    public Dictionary<string, ServiceConfig> Services { get; set; } = new();
    /// <summary>
    /// Networks dictionary.
    /// </summary>
    [YamlMember(Alias = "networks")]
    public Dictionary<string, NetworkConfig> Networks { get; set; } = new();
    /// <summary>
    /// Volumes dictionary.
    /// </summary>
    [YamlMember(Alias = "volumes")]
    public Dictionary<string, Dictionary<string, string?>>? Volumes { get; set; }
}