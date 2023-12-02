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

    private void ExecuteDown(Dictionary<string, ServiceConfig> targetServices, 
        ContainersConfig config, DockerManager dockerManager)
    {
        foreach ((string name, ServiceConfig service) in targetServices)
        {
            var exists = dockerManager.ContainerExists(service.CurrentName);
            if (!exists && !Purge)
            {
                PrintMessage($"<{name}> Container not fond, nothing to do!", _warningColor); 
                continue;
            }

            if (exists)
            {
                PrintMessage($"<{name}> Container found, removing it.");
            }
            else
            {
                PrintMessage($"<{name}> Container not found, trying to purge resources.", _warningColor);
            }
            
            Changes += dockerManager.PurgeContainer(name, config, Purge, IsDryRun);
            if (IsDryRun)
            {
                PrintMessage(exists
                        ? $"<{name}> Container remove finished."
                        : $"<{name}> Container resources purge finished.", 
                    _standardColor, separator: IsVerbose);
                PrintMessage("Dry run: No changes were made!", _warningColor);
            }
            else
            {
                PrintMessage(exists
                        ? $"<{name}> Container removed{(Purge ? " and resources purged" : "")} successfully!"
                        : $"<{name}> Container resources purged successfully!", 
                    _successColor, separator: IsVerbose);
            }
        }
    }

    private void ExecuteDowngrade(Dictionary<string, ServiceConfig> targetServices, DockerManager dockerManager)
    {
        foreach ((string name, ServiceConfig service) in targetServices)
        {
            if (string.IsNullOrEmpty(service.PreviousName))
            {
                PrintMessage($"<{name}> Previous name is null/empty, rollback not possible!", _warningColor);
                continue;
            }
            
            if (!dockerManager.ContainerExists(service.PreviousName))
            {
                PrintMessage($"<{name}> Previous container '{service.PreviousName}' not fond, rollback not possible!", _warningColor);
                continue;
            }
            
            PrintMessage($"<{name}> Previous container found, downgrading.");
            Changes += dockerManager.DowngradeContainer(service, IsDryRun);
            if (IsDryRun)
            {
                PrintMessage($"<{name}> Container downgrade finished.", _standardColor, separator: IsVerbose);
                PrintMessage("Dry run: No changes were made!", _warningColor);
            }
            else
            {
                PrintMessage($"<{name}> Container downgrading successfully!", _successColor, separator: IsVerbose);
            }
        }
    }
}