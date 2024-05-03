using Adeotek.Extensions.ConfigFiles;

using YamlDotNet.Serialization;

namespace Adeotek.Extensions.Containers.Config;

public class ServiceHealthCheckConfig
{
    /// <summary>
    /// Health check command array.
    /// e.g. ["CMD", "curl", "-f", "http://localhost"]
    /// </summary>
    [YamlMember(Alias = "test")]
    public StringArray Test { get; set; } = new();
    /// <summary>
    /// Health check run interval.
    /// </summary>
    [YamlMember(Alias = "interval")]
    public string Interval { get; set; } = "30s";
    /// <summary>
    /// Health check command timeout.
    /// </summary>
    [YamlMember(Alias = "timeout")]
    public string Timeout { get; set; } = "30s";
    /// <summary>
    /// Service initialization duration.
    /// Health check fails during this period will not be counted towards the maximum number of retries.
    /// </summary>
    [YamlMember(Alias = "start_period")]
    public string StartPeriod { get; set; } = "0s";
    /// <summary>
    /// Time between health checks during the start period.
    /// </summary>
    [YamlMember(Alias = "start_interval")]
    public string StartInterval { get; set; } = "5s";
    /// <summary>
    /// Health check retries.
    /// </summary>
    [YamlMember(Alias = "retries")]
    public int Retries { get; set; } = 3;
}