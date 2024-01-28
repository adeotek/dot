using System.Diagnostics.CodeAnalysis;

using Adeotek.Extensions.Containers.Config;
using Adeotek.Extensions.Containers.Exceptions;

namespace Adeotek.Extensions.Containers;

[ExcludeFromCodeCoverage]
public class ContainersManager : ContainersCli
{
    public ContainersManager(string cliFlavor = "docker", ContainersCliCommand? containersCli = null) 
        : base(cliFlavor, containersCli)
    {
    }
    
    public int RestartService(string containerName, bool dryRun = false) =>
        StopContainer(containerName, dryRun)
        + StartContainer(containerName, dryRun);
    
    public int StopAndRemoveService(string containerName, bool dryRun = false) =>
        StopContainer(containerName, dryRun)
        + RemoveContainer(containerName, dryRun);

    public int StopAndRenameService(string currentName, string newName, bool dryRun = false) =>
        StopContainer(currentName, dryRun)
        + RenameContainer(currentName, newName, dryRun);
    
    public int CheckAndCreateService(ServiceConfig service, List<NetworkConfig>? networks = null, 
        bool autoStart = true, bool dryRun = false)
    {
        var changes = CreateNetworksIfMissing(service, networks, dryRun)
            + (service.Volumes?.Sum(volume => CreateVolumeIfMissing(volume, dryRun)) ?? 0);
        
        if (service.Networks is null || service.Networks.Count < 2)
        {
            return changes + CreateContainer(service, networks, autoStart, dryRun);    
        }
        
        changes += CreateContainer(service, networks, autoStart, dryRun) 
                   + AttachContainerToNetworks(service, networks, dryRun);
        
        if (autoStart)
        {
            StartContainer(service.CurrentName, dryRun);
        }
        
        return changes;
    }

    public int UpgradeService(ServiceConfig service, List<NetworkConfig>? networks = null,
        bool replace = false, bool force = false, bool dryRun = false)
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
            ? StopAndRemoveService(service.CurrentName, dryRun) 
            : DemoteService(service, dryRun);
    
        changes += CheckAndCreateService(service, networks, dryRun: dryRun);
        if (dryRun)
        {
            LogMessage("Container create finished.", "msg");
            LogMessage("Dry run: No changes were made!", "warn");
            return changes;
        }
        
        LogMessage("Container updated successfully!", "msg");
        return changes;
    }
    
    public int DemoteService(ServiceConfig service, bool dryRun = false)
    {
        if (string.IsNullOrEmpty(service.PreviousName))
        {
            LogMessage($"<{service.ServiceName}> Demote not possible, no `BaseName`/`PreviousSuffix` defined!", "warn");
            return StopAndRemoveService(service.CurrentName, dryRun);
        }
        
        return (ContainerExists(service.PreviousName)
                   ? StopAndRemoveService(service.PreviousName, dryRun)
                   : 0)
               + StopAndRenameService(service.CurrentName, service.PreviousName, dryRun);
    }

    public int DowngradeService(ServiceConfig service, bool dryRun = false)
    {
        ContainersCliException.ThrowIfNull(service.PreviousName, "downgrade", 
            "Previous container name is null/empty!");
        ContainersCliException.ThrowIf(!ContainerExists(service.PreviousName), "downgrade",
            "Previous container is missing!");
        return (ContainerExists(service.CurrentName)
               ? StopAndRemoveService(service.CurrentName, dryRun)
               : 0)
           + RenameContainer(service.PreviousName, service.CurrentName, dryRun)
           + StartContainer(service.CurrentName, dryRun);
    }
    
    public int RemoveServiceContainers(ServiceConfig service, bool purge = false, bool dryRun = false)
    {
        var changes = StopAndRemoveService(service.CurrentName, dryRun);
        if (!purge)
        {
            return changes;
        }

        if (string.IsNullOrEmpty(service.PreviousName) || !ContainerExists(service.PreviousName))
        {
            return changes;
        }

        LogMessage("Previous container found, removing it.");
        changes += StopAndRemoveService(service.PreviousName, dryRun);
        return changes;
    }
    
    public int PurgeService(ServiceConfig service, ContainersConfig config, bool purge = false, bool dryRun = false)
    {
        var changes = RemoveServiceContainers(service, purge, dryRun);
        if (!purge)
        {
            return changes;
        }

        var otherServiceVolumes = config.GetAllVolumes(service.ServiceName);
        changes += service.Volumes?
            .Where(x => x.Type == "volume" 
                && otherServiceVolumes.All(e => e.Source != x.Source))
            .Sum(volume => RemoveVolume(volume.Source, dryRun))
            ?? 0;

        if (service.Networks is null)
        {
            return changes;
        }
        
        var otherServiceNetworks = config.GetAllServiceNetworks(service.ServiceName)
            .ToArray();
        foreach (var serviceNetwork in service.Networks!.ToServiceNetworkEnumerable())
        {
            var network = config.Networks.GetByName(serviceNetwork.NetworkName);
            if (network is null || network.External
                || otherServiceNetworks
                    .Any(x => x.NetworkName == serviceNetwork.NetworkName))
            {
                continue;
            }

            changes += RemoveNetwork(network.Name, dryRun);
        }
        
        return changes;
    }

    public int PurgeVolumes(List<ServiceConfig> targetServices, ContainersConfig config, bool dryRun)
    {
        List<VolumeConfig> volumes = new();
        var unaffectedServices = config.Services
            .Where(x => 
                targetServices.All(t => t.ServiceName != x.Key)
                && x.Value.Volumes is not null && x.Value.Volumes.Length > 0)
            .ToArray(); 
        foreach (var service in targetServices.Where(x => x.Volumes is not null && x.Volumes.Length > 0))
        {
            foreach (var volume in service.Volumes!)
            {
                var isShared = unaffectedServices.Any(x =>
                    x.Value.Volumes?.Any(t => t.Type == volume.Type && t.Source == volume.Source) ?? false);
                
                //// Commented out in case we want to purge bind volumes in the future
                // if (volume.Type == "bind" && (volume.Bind?.CreateHostPath ?? false) && !isShared)
                // {
                //     volumes.Add(volume);
                //     continue;
                // }
                
                if (volume.Type == "volume" && !isShared)
                {
                    volumes.Add(volume);
                }
            }
        }

        return volumes.Sum(volume => RemoveVolume(volume.Source, dryRun));
    }
    
    public int PurgeNetworks(List<ServiceConfig> targetServices, ContainersConfig config, bool dryRun)
    {
        List<string> networks = new();
        var unaffectedServices = config.Services
            .Where(x => 
                targetServices.All(t => t.ServiceName != x.Key)
                && x.Value.Networks is not null && x.Value.Networks.Count > 0)
            .ToArray(); 
        foreach (var service in targetServices.Where(x => x.Networks is not null && x.Networks.Count > 0))
        {
            foreach ((string? networkKey, ServiceNetworkConfig? _) in service.Networks!)
            {
                if (unaffectedServices.Any(x =>
                        x.Value.Networks?.Any(t => t.Key == networkKey) ?? false))
                {
                    continue;
                }
                
                networks.Add(networkKey);
            }
        }

        return config.Networks
            .Where(x => networks.Contains(x.Key) && !x.Value.External)
            .Sum(x => RemoveNetwork(x.Value.Name, dryRun));
    }

    public int CreateVolumeIfMissing(VolumeConfig volume, bool dryRun = false)
    {
        switch (volume.Type)
        {
            case "bind":
                if (Path.Exists(volume.Source))
                {
                    return 0;
                }
    
                if (!(volume.Bind?.CreateHostPath ?? false))
                {
                    throw new ContainersCliException("create container", 1, $"Docker volume '{volume.Source}' is missing or cannot be created!");
                }

                return CreateBindVolume(volume, dryRun);
            case "volume":
                return VolumeExists(volume.Source) 
                    ? 0 
                    : CreateVolume(volume.Source, dryRun);

            default:
                throw new NotImplementedException($"Unsupported volume type: `{volume.Type}`!");
        }
    }

    public int BackupVolume(VolumeConfig volume, string backupLocation, out string? archiveFile, bool dryRun = false)
    {
        if (volume.SkipBackup)
        {
            archiveFile = null;
            return 0;
        }

        archiveFile = Path.Combine(backupLocation, $"{volume.BackupName}-{DateTime.Now:yyyyMMddHHmmss}.tar.gz");
        ContainersCliException.ThrowIfNull(archiveFile, "volume backup",
            $"Invalid backup destination for volume: {volume.Source}.");
        return volume.Type switch
        {
            "bind" => Directory.Exists(volume.Source)
                ? ArchiveDirectory(volume.Source, archiveFile, dryRun) ? 1 : 0
                : 0,
            "volume" => VolumeExists(volume.Source) ? ArchiveVolume(volume.Source, archiveFile, dryRun) ? 1 : 0 : 0,
            _ => throw new NotImplementedException($"Unsupported volume type: `{volume.Type}`!")
        };
    }

    public int CreateNetworksIfMissing(ServiceConfig service, List<NetworkConfig>? networks, bool dryRun = false)
    {
        if (service.Networks is null || service.Networks.Count == 0)
        {
            return 0;
        }
        
        var changes = 0;
        foreach ((string networkKey, _) in service.Networks)
        {
            var network = networks?.FirstOrDefault(x => x.NetworkName == networkKey);
            ContainersCliException.ThrowIfNull(network, "network create", 
                $"Network {networkKey} not defined, but used for service: {service.ServiceName}.");
            changes += CreateNetworkIfMissing(network, dryRun);
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

    public int AttachContainerToNetworks(ServiceConfig service, List<NetworkConfig>? networks, bool dryRun = false)
    {
        if (service.Networks is null || service.Networks.Count < 2)
        {
            return 0;
        }

        var changes = 0;
        foreach ((string networkKey, ServiceNetworkConfig? serviceNetwork) in service.Networks.Skip(1))
        {
            var network = networks?.FirstOrDefault(x => x.NetworkName == networkKey);
            ContainersCliException.ThrowIfNull(network, "network connect", 
                $"Network {networkKey} not defined, but used for service: {service.ServiceName}.");
            changes += AttachContainerToNetwork(service.CurrentName, network.Name, serviceNetwork, dryRun);
        }

        return changes;
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
