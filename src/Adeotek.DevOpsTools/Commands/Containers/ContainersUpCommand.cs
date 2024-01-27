using Adeotek.DevOpsTools.CommandsSettings.Containers;
using Adeotek.Extensions.Containers.Config;

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
        var containersManager = GetContainersManager();
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
            
            if (containersManager.ContainerExists(service.CurrentName))
            {
                if (!Upgrade)
                {
                    PrintMessage($"<{service.ServiceName}> Container already present, nothing to do!", _successColor, separator: IsVerbose);
                    continue;
                }

                if (Backup)
                {
                    BackupServiceVolumes(service, _settings?.BackupLocation, containersManager);    
                }
        
                PrintMessage($"<{service.ServiceName}> Container already present, updating it.", _warningColor);
                Changes += containersManager.UpgradeService(service, networks, Replace, Force, IsDryRun);
                continue;
            }
        
            PrintMessage($"<{service.ServiceName}> Container not fond, creating new one.");
            Changes += containersManager.CheckAndCreateService(service, networks, AutoStart, IsDryRun);
        
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
