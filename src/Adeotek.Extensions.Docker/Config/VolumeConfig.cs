namespace Adeotek.Extensions.Docker.Config;

public class VolumeConfig
{
    /// <summary>
    /// The mount type. Either volume, bind, tmpfs, npipe, or cluster
    /// </summary>
    public string Type { get; set; } = "volume";
    /// <summary>
    /// The source of the mount, a path on the host for a bind mount,
    /// or the name of a volume defined in the top-level volumes key.
    /// Not applicable for a tmpfs mount.
    /// </summary>
    public string Source { get; set; } = default!;
    /// <summary>
    /// The path in the container where the volume is mounted.
    /// </summary>
    public string Target { get; set; } = default!;
    /// <summary>
    /// Flag to set the volume as read-only.
    /// </summary>
    public bool ReadOnly { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public VolumeBindConfig? Bind { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public VolumeVolumeConfig? Volume { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public VolumeTmpFsConfig? TmpFs { get; set; }
}