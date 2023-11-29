namespace Adeotek.Extensions.Docker.Config;

public class VolumeTmpFsConfig
{
    /// <summary>
    /// The size for the tmpfs mount in bytes (either numeric or as bytes unit).
    /// </summary>
    public string? Size { get; set; }
    /// <summary>
    /// The file mode for the tmpfs mount as Unix permission bits as an octal number.
    /// </summary>
    public string? Mode { get; set; }
}