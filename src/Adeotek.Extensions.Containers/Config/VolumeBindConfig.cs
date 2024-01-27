using YamlDotNet.Serialization;

namespace Adeotek.Extensions.Containers.Config;

public class VolumeBindConfig
{
    /// <summary>
    /// The propagation mode used for the bind.
    /// </summary>
    [YamlMember(Alias = "propagation")]
    public string? Propagation { get; set; }
    /// <summary>
    /// Creates a directory at the source path on host if there is nothing present.
    /// Does nothing if there is something present at the path.
    /// </summary>
    [YamlMember(Alias = "create_host_path")]
    public bool CreateHostPath { get; set; }
    /// <summary>
    /// The SELinux re-labeling option z (shared) or Z (private)
    /// </summary>
    [YamlMember(Alias = "selinux")]
    public string? SeLinux { get; set; }
}