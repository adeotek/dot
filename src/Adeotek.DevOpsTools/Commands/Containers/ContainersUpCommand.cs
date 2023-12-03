using Adeotek.DevOpsTools.CommandsSettings.Containers;
using Adeotek.Extensions.Docker.Config;

namespace Adeotek.DevOpsTools.Commands.Containers;

internal sealed class ContainersUpCommand : ContainersBaseCommand<ContainersUpSettings>
{
    protected override string ResultLabel => "Changes";
    private bool Upgrade => _settings?.Upgrade ?? false;
    private bool Replace => _settings?.Replace ?? false;
    private bool Force => _settings?.Force ?? false;
    
    protected override void ExecuteContainerCommand(ContainersConfig config)
    {
        var dockerManager = GetDockerManager();
        foreach (var service in GetTargetServices(config))
        {
            if (dockerManager.ContainerExists(service.CurrentName))
            {
                if (!Upgrade)
                {
                    PrintMessage($"<{service.ServiceName}> Container already present, nothing to do!", _successColor, separator: IsVerbose);
                    return;
                }
        
                PrintMessage($"<{service.ServiceName}> Container already present, updating it.", _warningColor);
                Changes += dockerManager.UpgradeService(service, Replace, Force, IsDryRun);
                return;
            }
        
            PrintMessage($"<{service.ServiceName}> Container not fond, creating new one.");
            Changes += dockerManager.CheckAndCreateService(service, config.Networks.ToNetworksEnumerable().ToList(), IsDryRun);
        
            if (Changes == 0)
            {
                PrintMessage($"<{service.ServiceName}> Command failed, the container was not created!", _errorColor);
                return;
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
