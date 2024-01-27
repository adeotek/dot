using YamlDotNet.Serialization;

namespace Adeotek.Extensions.Containers.Config;

public class VolumeTmpFsConfig
{
    /// <summary>
    /// The size for the tmpfs mount in bytes (either numeric or as bytes unit).
    /// </summary>
    [YamlMember(Alias = "size")]
    public string? Size { get; set; }
    /// <summary>
    /// The file mode for the tmpfs mount as Unix permission bits as an octal number.
    /// </summary>
    [YamlMember(Alias = "mode")]
    public string? Mode { get; set; }
}