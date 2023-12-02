using System.Diagnostics.CodeAnalysis;

using Adeotek.Extensions.Docker.Config;
using Adeotek.Extensions.Docker.Exceptions;

namespace Adeotek.Extensions.Docker;

[ExcludeFromCodeCoverage]
public class DockerManager : DockerCli
{
    public int RestartContainer(string containerName, bool dryRun = false) =>
        StopContainer(containerName, dryRun)
        + StartContainer(containerName, dryRun);
    
    public int StopAndRemoveContainer(string containerName, bool dryRun = false) =>
        StopContainer(containerName, dryRun)
        + RemoveContainer(containerName, dryRun);

    public int StopAndRenameContainer(string currentName, string newName, bool dryRun = false) =>
        StopContainer(currentName, dryRun)
        + RenameContainer(currentName, newName, dryRun);
    
    public int CheckAndCreateContainer(ServiceConfig service, 
        Dictionary<string, NetworkConfig>? networks = null, bool dryRun = false) =>
        (networks is null ? 0 : CreateNetworksIfMissing(service, networks, dryRun))
         + (service.Volumes?.Sum(volume => CreateVolumeIfMissing(volume, dryRun)) ?? 0)
         + CreateContainer(service, dryRun);

    public int UpgradeContainer(ServiceConfig service, bool replace = false, bool force = false, bool dryRun = false)
    {
        if (!CheckIfNewVersionExists(service))
        {
            if (!force)
            {
                LogMessage("No newer version found, nothing to do.", "msg");
                return 0;    
            }
            LogMessage("No newer version found, forcing container recreation!", "warn");
        }

        var changes = replace 
            ? StopAndRemoveContainer(service.CurrentName, dryRun) 
            : DemoteContainer(service, dryRun);
    
        changes += CheckAndCreateContainer(service, dryRun: dryRun);
        if (dryRun)
        {
            LogMessage("Container create finished.", "msg");
            LogMessage("Dry run: No changes were made!", "warn");
            return changes;
        }
        
        LogMessage("Container updated successfully!", "msg");
        return changes;
    }
    
    public int DemoteContainer(ServiceConfig service, bool dryRun = false)
    {
        DockerCliException.ThrowIfNull(service.PreviousName, "demote", 
            "Previous container name is null/empty!");
        return (ContainerExists(service.PreviousName)
                   ? StopAndRemoveContainer(service.PreviousName, dryRun)
                   : 0)
               + StopAndRenameContainer(service.CurrentName, service.PreviousName, dryRun);
    }

    public int DowngradeContainer(ServiceConfig service, bool dryRun = false)
    {
        DockerCliException.ThrowIfNull(service.PreviousName, "downgrade", 
            "Previous container name is null/empty!");
        DockerCliException.ThrowIf(!ContainerExists(service.PreviousName), "downgrade",
            "Previous container is missing!");
        return (ContainerExists(service.CurrentName)
               ? StopAndRemoveContainer(service.CurrentName, dryRun)
               : 0)
           + RenameContainer(service.PreviousName, service.CurrentName, dryRun)
           + StartContainer(service.CurrentName, dryRun);
    }
    
    public int PurgeContainer(string serviceName, ContainersConfig config, bool purge = false, bool dryRun = false)
    {
        var changes = StopAndRemoveContainer(config.Services[serviceName].CurrentName, dryRun);
        if (!purge)
        {
            return changes;
        }

        var previousName = config.Services[serviceName].PreviousName;
        if (!string.IsNullOrEmpty(previousName) && ContainerExists(previousName))
        {
            LogMessage("Previous container found, removing it.");
            changes += StopAndRemoveContainer(previousName, dryRun);
        }

        changes += config.Volumes
            .Where(e => e is { AutoCreate: true, IsBind: false })
            .Sum(volume => RemoveVolume(volume.Source, dryRun));

        if (config.Network is not null && !config.Network.IsShared)
        {
            changes += RemoveNetwork(config.Network.Name, dryRun);
        }

        return changes;
    }

    public int CreateVolumeIfMissing(VolumeConfig volume, bool dryRun = false)
    {
        if (volume.IsBind)
        {
            if (Path.Exists(volume.Source))
            {
                return 0;
            }
    
            if (!volume.AutoCreate)
            {
                throw new DockerCliException("create container", 1, $"Docker volume '{volume.Source}' is missing or cannot be created!");
            }

            return CreateBindVolume(volume, dryRun);
        }
        
        if (VolumeExists(volume.Source))
        {
            return 0;
        }
    
        if (!volume.AutoCreate)
        {
            throw new DockerCliException("create container", 1, $"Docker volume '{volume.Source}' is missing or cannot be created!");
        }

        return CreateVolume(volume.Source, dryRun);
    }

    public int CreateNetworksIfMissing(ServiceConfig service, Dictionary<string, NetworkConfig>? networks, bool dryRun = false)
    {
        if (service.Networks is null || service.Networks.Count == 0)
        {
            return 0;
        }
        
        var changes = 0;
        foreach ((string name, ServiceNetworkConfig serviceNetwork) in service.Networks)
        {
            var network = networks?.FirstOrDefault(x => x.Key == name);
            DockerCliException.ThrowIfNull(network, "network create", 
                $"Network {name} not defined, but used for service: {service.ServiceName}.");
            changes += CreateNetworkIfMissing(network.Value.Value, dryRun);
        }
        return changes;
    }
    
    public int CreateNetworkIfMissing(NetworkConfig? network, bool dryRun = false)
    {
        if (network is null || string.IsNullOrEmpty(network.Name) || NetworkExists(network.Name))
        {
            return 0;
        }

        return CreateNetwork(network, dryRun);
    }
    
    public bool CheckIfNewVersionExists(ServiceConfig service)
    {
        PullImage(service.Image);
        var imageId = GetImageId(service.Image);
        var containerImageId = GetContainerImageId(service.CurrentName);
        return !string.IsNullOrEmpty(imageId) 
            && !imageId.Equals(containerImageId, StringComparison.InvariantCultureIgnoreCase);
    }
}
