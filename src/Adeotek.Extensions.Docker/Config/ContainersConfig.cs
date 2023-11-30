using YamlDotNet.Serialization;

namespace Adeotek.Extensions.Docker.Config;

public class ContainersConfig
{
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
}