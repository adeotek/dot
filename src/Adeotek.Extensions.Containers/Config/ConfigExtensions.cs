using System.Diagnostics.CodeAnalysis;

namespace Adeotek.Extensions.Containers.Config;

[ExcludeFromCodeCoverage]
public static class ConfigExtensions
{
    public static IEnumerable<ServiceConfig> ToServicesEnumerable(this Dictionary<string, ServiceConfig> services) =>
        services.Select(x => x.Value.SetServiceName(x.Key));

    public static IEnumerable<NetworkConfig> ToNetworksEnumerable(this Dictionary<string, NetworkConfig> networks) =>
        networks.Select(x => x.Value.SetNetworkName(x.Key));
    
    public static IEnumerable<ServiceNetworkConfig> ToServiceNetworkEnumerable(this Dictionary<string, ServiceNetworkConfig?>? networks) =>
        networks?.Select(x => (x.Value ?? new ServiceNetworkConfig()).SetNetworkName(x.Key))
            ?? Array.Empty<ServiceNetworkConfig>();
    
    public static ServiceConfig? GetByName(this Dictionary<string, ServiceConfig> services, string name)
    {
        var item= services.FirstOrDefault(x => x.Key == name);
        return item.Value?.SetServiceName(item.Key);
    }
    
    public static NetworkConfig? GetByName(this Dictionary<string, NetworkConfig> networks, string name)
    {
        var item= networks.FirstOrDefault(x => x.Key == name);
        return item.Value?.SetNetworkName(item.Key);
    }

    public static IEnumerable<VolumeConfig> GetAllVolumes(this ContainersConfig config, string? excludeService = null)
    {
        var volumes = new List<VolumeConfig>();
        foreach ((string name, ServiceConfig service) in config.Services)
        {
            if (service.Volumes is null || service.Volumes.Length == 0
                || (!string.IsNullOrEmpty(excludeService) && name == excludeService))
            {
                continue;
            }
            
            volumes.AddRange(service.Volumes
                .Where(x => x.Type != "volume" 
                    && volumes.All(e => e.Source != x.Source))
            );
        }

        return volumes;
    }
    
    public static IEnumerable<ServiceNetworkConfig> GetAllServiceNetworks(this ContainersConfig config, string? excludeService = null)
    {
        var networks = new List<ServiceNetworkConfig>();
        foreach ((string name, ServiceConfig service) in config.Services)
        {
            if (service.Networks is null || service.Networks.Count == 0
                || (!string.IsNullOrEmpty(excludeService) && name == excludeService))
            {
                continue;
            }
            
            networks.AddRange(service.Networks!.ToServiceNetworkEnumerable()
                .Where(x => networks.All(e => e.NetworkName != x.NetworkName))
            );
        }

        return networks;
    }
}