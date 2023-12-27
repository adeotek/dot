using System.Diagnostics.CodeAnalysis;

using Adeotek.Extensions.Docker.Config;
using Adeotek.Extensions.Docker.Exceptions;

namespace Adeotek.Extensions.Docker;

[ExcludeFromCodeCoverage]
public class DockerManager : DockerCli
{
    public DockerManager(DockerCliCommand? dockerCli = null) : base(dockerCli)
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
    
    public int CheckAndCreateService(ServiceConfig service, 
        List<NetworkConfig>? networks = null, bool dryRun = false) =>
        (networks is null ? 0 : CreateNetworksIfMissing(service, networks, dryRun))
         + (service.Volumes?.Sum(volume => CreateVolumeIfMissing(volume, dryRun)) ?? 0)
         + CreateContainer(service, networks, dryRun);

    public int UpgradeService(ServiceConfig service, bool replace = false, bool force = false, bool dryRun = false)
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
    
        changes += CheckAndCreateService(service, dryRun: dryRun);
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
        DockerCliException.ThrowIfNull(service.PreviousName, "demote", 
            "Previous container name is null/empty!");
        return (ContainerExists(service.PreviousName)
                   ? StopAndRemoveService(service.PreviousName, dryRun)
                   : 0)
               + StopAndRenameService(service.CurrentName, service.PreviousName, dryRun);
    }

    public int DowngradeService(ServiceConfig service, bool dryRun = false)
    {
        DockerCliException.ThrowIfNull(service.PreviousName, "downgrade", 
            "Previous container name is null/empty!");
        DockerCliException.ThrowIf(!ContainerExists(service.PreviousName), "downgrade",
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
        
        var otherServiceNetworks = config.GetAllServiceNetworks(service.ServiceName)
            .ToArray();
        foreach (var serviceNetwork in service.Networks.ToServiceNetworkEnumerable())
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
                
                if (volume.Type == "volume" && !isShared)
                {
                    volumes.Add(volume);
                    continue;
                }
                
                if (volume.Type == "bind" && (volume.Bind?.CreateHostPath ?? false) && !isShared)
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
                    throw new DockerCliException("create container", 1, $"Docker volume '{volume.Source}' is missing or cannot be created!");
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

    public int BackupVolume(VolumeConfig volume, string backupLocation, bool dryRun = false)
    {
        if (volume.SkipBackup)
        {
            return 0;
        }

        var archiveFile = Path.Combine(backupLocation, $"{volume.BackupName}-{DateTime.Now:yyyyMMddHHmmss}.tar.gz");
        DockerCliException.ThrowIfNull(archiveFile, "volume backup",
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
        foreach (var serviceNetwork in service.Networks.ToServiceNetworkEnumerable())
        {
            var network = networks?.FirstOrDefault(x => x.NetworkName == serviceNetwork.NetworkName);
            DockerCliException.ThrowIfNull(network, "network create", 
                $"Network {serviceNetwork.NetworkName} not defined, but used for service: {service.ServiceName}.");
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
    
    public bool CheckIfNewVersionExists(ServiceConfig service)
    {
        PullImage(service.Image);
        var imageId = GetImageId(service.Image);
        var containerImageId = GetContainerImageId(service.CurrentName);
        return !string.IsNullOrEmpty(imageId) 
            && !imageId.Equals(containerImageId, StringComparison.InvariantCultureIgnoreCase);
    }
}
