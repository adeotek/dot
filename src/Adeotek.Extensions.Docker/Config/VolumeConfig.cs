using System.Text.Json.Serialization;

using YamlDotNet.Serialization;

namespace Adeotek.Extensions.Docker.Config;

/// <summary>
/// Container volume.
/// Short syntax: VOLUME:CONTAINER_PATH:ACCESS_MODE
/// - VOLUME: Can be either a host path on the platform hosting containers (bind mount) or a volume name.
/// - CONTAINER_PATH: The path in the container where the volume is mounted.
/// - ACCESS_MODE: A comma-separated, list of options:
///     rw: Read and write access. This is the default if none is specified.
///     ro: Read-only access.
///     z: SELinux option indicating that the bind mount host content is shared among multiple containers.
///     Z: SELinux option indicating that the bind mount host content is private and unshared for other containers.
/// </summary>
public class VolumeConfig
{
    // public static readonly string[] VolumeType = { "volume", "bind", "tmpfs", "npipe", "cluster" };
    
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
    /// <summary>
    /// If TRUE, the volume will not be backed up when running the backup command.
    /// [NOT SUPPORTED by Docker Compose]
    /// </summary>
    [YamlMember(Alias="skip_backup")]
    public bool SkipBackup { get; set; }
    
    // Computed
    [JsonIgnore]
    [YamlIgnore]
    public string BackupName =>
        Type == "volume"
            ? Source
            : Source.Replace('/', '-').Replace('\\', '-');
}