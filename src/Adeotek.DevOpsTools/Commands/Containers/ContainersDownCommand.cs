using Adeotek.DevOpsTools.CommandsSettings.Containers;
using Adeotek.Extensions.Docker;
using Adeotek.Extensions.Docker.Config;

namespace Adeotek.DevOpsTools.Commands.Containers;

internal sealed class ContainersDownCommand : ContainersBaseCommand<ContainersDownSettings>
{
    protected override string ResultLabel => "Changes";
    private bool Downgrade => _settings?.Downgrade ?? false;
    private bool Purge => _settings?.Purge ?? false;
    
    protected override void ExecuteContainerCommand(ContainersConfig config)
    {
        var dockerManager = GetDockerManager();
        
        if (Downgrade)
        {
            ExecuteDowngrade(GetTargetServices(config), dockerManager);
            return;
        }

        ExecuteDown(GetTargetServices(config), config, dockerManager);
    }

    private void ExecuteDown(List<ServiceConfig> targetServices, 
        ContainersConfig config, DockerManager dockerManager)
    {
        foreach (var service in targetServices)
        {
            var exists = dockerManager.ContainerExists(service.CurrentName);
            if (!exists && !Purge)
            {
                PrintMessage($"<{service.ServiceName}> Container not fond, nothing to do!", _warningColor); 
                continue;
            }

            if (exists)
            {
                PrintMessage($"<{service.ServiceName}> Container found, removing it.");
            }
            else
            {
                PrintMessage($"<{service.ServiceName}> Container not found, trying to purge resources.", _warningColor);
            }

            if (targetServices.Count == 1)
            {
                Changes += dockerManager.PurgeService(service, config, Purge, IsDryRun);
            }
            else
            {
                Changes += dockerManager.RemoveServiceContainers(service, Purge, IsDryRun);
            }
            
            if (IsDryRun)
            {
                PrintMessage(Purge
                        ? $"<{service.ServiceName}> Container purge finished."
                        : $"<{service.ServiceName}> Container remove finished.", 
                    _standardColor, separator: IsVerbose);
                PrintMessage("Dry run: No changes were made!", _warningColor);
            }
            else
            {
                PrintMessage(Purge
                        ? $"<{service.ServiceName}> Container purged successfully!"
                        : $"<{service.ServiceName}> Container removed successfully!", 
                    _successColor, separator: IsVerbose);
            }
        }

        ExecuteVolumesPurge(targetServices, config, dockerManager);
        ExecuteNetworksPurge(targetServices, config, dockerManager);
    }

    private void ExecuteDowngrade(List<ServiceConfig> targetServices, DockerManager dockerManager)
    {
        foreach (var service in targetServices)
        {
            if (string.IsNullOrEmpty(service.PreviousName))
            {
                PrintMessage($"<{service.ServiceName}> Previous name is null/empty, rollback not possible!", _warningColor);
                continue;
            }
            
            if (!dockerManager.ContainerExists(service.PreviousName))
            {
                PrintMessage($"<{service.ServiceName}> Previous container '{service.PreviousName}' not fond, rollback not possible!", _warningColor);
                continue;
            }
            
            PrintMessage($"<{service.ServiceName}> Previous container found, downgrading.");
            Changes += dockerManager.DowngradeService(service, IsDryRun);
            if (IsDryRun)
            {
                PrintMessage($"<{service.ServiceName}> Container downgrade finished.", _standardColor, separator: IsVerbose);
                PrintMessage("Dry run: No changes were made!", _warningColor);
            }
            else
            {
                PrintMessage($"<{service.ServiceName}> Container downgrading successfully!", _successColor, separator: IsVerbose);
            }
        }
    }

    private void ExecuteVolumesPurge(List<ServiceConfig> targetServices, ContainersConfig config,
        DockerManager dockerManager)
    {
        if (!Purge || targetVolumes.Count == 0)
        {
            return;
        }
        
        Changes += dockerManager.PurgeVolumes(targetVolumes, config, IsDryRun);
        if (IsDryRun)
        {
            PrintMessage("Volumes purge finished.", _standardColor, separator: IsVerbose);
            PrintMessage("Dry run: No changes were made!", _warningColor);
        }
        else
        {
            PrintMessage("Volumes purged successfully!", _successColor, separator: IsVerbose);
        }
    }
    
    private void ExecuteNetworksPurge(List<ServiceConfig> targetServices, ContainersConfig config,
        DockerManager dockerManager)
    {
        if (!Purge || targetNetworks.Count == 0)
        {
            return;
        }
        
        Changes += dockerManager.PurgeNetworks(targetNetworks, config, IsDryRun);
        if (IsDryRun)
        {
            PrintMessage("Networks purge finished.", _standardColor, separator: IsVerbose);
            PrintMessage("Dry run: No changes were made!", _warningColor);
        }
        else
        {
            PrintMessage("Networks purged successfully!", _successColor, separator: IsVerbose);
        }
    }
}