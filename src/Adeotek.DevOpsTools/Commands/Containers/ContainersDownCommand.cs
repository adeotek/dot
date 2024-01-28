using Adeotek.DevOpsTools.CommandsSettings.Containers;
using Adeotek.Extensions.Containers;
using Adeotek.Extensions.Containers.Config;

namespace Adeotek.DevOpsTools.Commands.Containers;

internal sealed class ContainersDownCommand : ContainersBaseCommand<ContainersDownSettings>
{
    protected override string ResultLabel => "Changes";
    private bool Downgrade => _settings?.Downgrade ?? false;
    private bool Purge => _settings?.Purge ?? false;
    private bool Backup => _settings?.Backup ?? false;
    
    protected override void ExecuteContainerCommand(ContainersConfig config)
    {
        var containersManager = GetContainersManager();

        if (Downgrade)
        {
            ExecuteDowngrade(GetTargetServices(config, _settings?.ServiceName), containersManager);
        }
        else
        {
            ExecuteDown(GetTargetServices(config, _settings?.ServiceName), config, containersManager);    
        }
    }

    private void ExecuteDown(List<ServiceConfig> targetServices, 
        ContainersConfig config, ContainersManager containersManager)
    {
        var first = true;
        foreach (var service in targetServices)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                PrintSeparator();
            }
            
            var exists = containersManager.ContainerExists(service.CurrentName);
            if (!exists && !Purge)
            {
                PrintMessage($"<{service.ServiceName}> Container not fond, nothing to do!", _warningColor); 
                continue;
            }
            
            if (Backup)
            {
                BackupServiceVolumes(service, _settings?.BackupLocation, containersManager);    
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
                Changes += containersManager.PurgeService(service, config, Purge, IsDryRun);
            }
            else
            {
                Changes += containersManager.RemoveServiceContainers(service, Purge, IsDryRun);
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

        ExecuteVolumesPurge(targetServices, config, containersManager);
        ExecuteNetworksPurge(targetServices, config, containersManager);
    }

    private void ExecuteDowngrade(List<ServiceConfig> targetServices, ContainersManager containersManager)
    {
        var first = true;
        foreach (var service in targetServices)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                PrintSeparator();
            }
            
            if (string.IsNullOrEmpty(service.PreviousName))
            {
                PrintMessage($"<{service.ServiceName}> Previous name is null/empty, rollback not possible!", _warningColor);
                continue;
            }
            
            if (!containersManager.ContainerExists(service.PreviousName))
            {
                PrintMessage($"<{service.ServiceName}> Previous container '{service.PreviousName}' not fond, rollback not possible!", _warningColor);
                continue;
            }

            if (Backup)
            {
                BackupServiceVolumes(service, _settings?.BackupLocation, containersManager);    
            }
            
            PrintMessage($"<{service.ServiceName}> Previous container found, downgrading.");
            Changes += containersManager.DowngradeService(service, IsDryRun);
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
        ContainersManager containersManager)
    {
        if (!Purge || targetServices.Count == 1)
        {
            return;
        }
        
        Changes += containersManager.PurgeVolumes(targetServices, config, IsDryRun);
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
        ContainersManager containersManager)
    {
        if (!Purge || targetServices.Count == 1)
        {
            return;
        }
        
        Changes += containersManager.PurgeNetworks(targetServices, config, IsDryRun);
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