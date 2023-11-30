using YamlDotNet.Serialization;

namespace Adeotek.Extensions.Docker.Config;

public class VolumeConfig
{
    public static readonly string[] VolumeType = { "volume", "bind", "tmpfs", "npipe", "cluster" };
    
    /// <summary>
    /// The mount type. Either volume, bind, tmpfs, npipe, or cluster
    /// </summary>
    [YamlMember(Alias = "type")]
    public string Type { get; set; } = default!;
    /// <summary>
    /// The source of the mount, a path on the host for a bind mount,
    /// or the name of a volume defined in the top-level volumes key.
    /// Not applicable for a tmpfs mount.
    /// </summary>
    [YamlMember(Alias = "source")]
    public string Source { get; set; } = default!;
    /// <summary>
    /// The path in the container where the volume is mounted.
    /// </summary>
    [YamlMember(Alias = "target")]
    public string Target { get; set; } = default!;
    /// <summary>
    /// Flag to set the volume as read-only.
    /// </summary>
    [YamlMember(Alias = "read_only")]
    public bool ReadOnly { get; set; }
    /// <summary>
    /// Configures additional `bind` options.
    /// </summary>
    [YamlMember(Alias = "bind")]
    public VolumeBindConfig? Bind { get; set; }
    /// <summary>
    /// Configures additional `volume` options.
    /// </summary>
    [YamlMember(Alias = "volume")]
    public VolumeVolumeConfig? Volume { get; set; }
    /// <summary>
    /// Configures additional `tmpfs` options.
    /// </summary>
    [YamlMember(Alias = "tmpfs")]
    public VolumeTmpFsConfig? TmpFs { get; set; }
}