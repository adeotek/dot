using YamlDotNet.Serialization;

namespace Adeotek.Extensions.Containers.Config;

public class VolumeVolumeConfig
{
    /// <summary>
    /// Flag to disable copying of data from a container when a volume is created.
    /// </summary>
    [YamlMember(Alias = "nocopy")]
    public bool NoCopy { get; set; }
}