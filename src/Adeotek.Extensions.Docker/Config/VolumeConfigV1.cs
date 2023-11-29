namespace Adeotek.Extensions.Docker.Config;

public class VolumeConfigV1
{
    public string Source { get; set; } = default!;
    public string Destination { get; set; } = default!;
    public bool IsBind { get; set; }
    public bool IsReadonly { get; set; }
    public bool AutoCreate { get; set; }
}