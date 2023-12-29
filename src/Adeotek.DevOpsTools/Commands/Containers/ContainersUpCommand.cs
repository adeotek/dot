using Adeotek.DevOpsTools.CommandsSettings.Containers;
using Adeotek.Extensions.Docker.Config;

namespace Adeotek.DevOpsTools.Commands.Containers;

internal sealed class ContainersUpCommand : ContainersBaseCommand<ContainersUpSettings>
{
    protected override string ResultLabel => "Changes";
    private bool Upgrade => _settings?.Upgrade ?? false;
    private bool Replace => _settings?.Replace ?? false;
    private bool Force => _settings?.Force ?? false;
    private bool Backup => _settings?.Backup ?? false;
    private bool AutoStart => !(_settings?.DoNotStart ?? false);
    
    protected override void ExecuteContainerCommand(ContainersConfig config)
    {
        var dockerManager = GetDockerManager();
        var networks = config.Networks.ToNetworksEnumerable().ToList();
        var first = true;
        foreach (var service in GetTargetServices(config, _settings?.ServiceName))
        {
            if (first)
            {
                first = false;
            }
            else
            {
                PrintSeparator();
            }
            
            if (dockerManager.ContainerExists(service.CurrentName))
            {
                if (!Upgrade)
                {
                    PrintMessage($"<{service.ServiceName}> Container already present, nothing to do!", _successColor, separator: IsVerbose);
                    continue;
                }
                
                if (Backup && service.Volumes is not null && service.Volumes.Length > 0)
                {
                    Changes += service.Volumes
                        .Sum(x => dockerManager.BackupVolume(x, _settings?.BackupLocation ?? "", IsDryRun));
        
                    if (IsDryRun)
                    {
                        PrintMessage($"<{service.ServiceName}> Volumes backup finished.", _standardColor, separator: IsVerbose);
                        PrintMessage("Dry run: No changes were made!", _warningColor);
                    }
                    else
                    {
                        PrintMessage($"<{service.ServiceName}> Volumes backup done!", _successColor, separator: IsVerbose);
                    }
                }
        
                PrintMessage($"<{service.ServiceName}> Container already present, updating it.", _warningColor);
                Changes += dockerManager.UpgradeService(service, networks, Replace, Force, IsDryRun);
                continue;
            }
        
            PrintMessage($"<{service.ServiceName}> Container not fond, creating new one.");
            Changes += dockerManager.CheckAndCreateService(service, networks, AutoStart, IsDryRun);
        
            if (Changes == 0)
            {
                PrintMessage($"<{service.ServiceName}> Command failed, the container was not created!", _errorColor);
                continue;
            }
        
            if (IsDryRun)
            {
                PrintMessage($"<{service.ServiceName}> Container create finished.", _standardColor, separator: IsVerbose);
                PrintMessage("Dry run: No changes were made!", _warningColor);
            }
            else
            {
                PrintMessage($"<{service.ServiceName}> Container created successfully!", _successColor, separator: IsVerbose);
            }
        }
    }
}
