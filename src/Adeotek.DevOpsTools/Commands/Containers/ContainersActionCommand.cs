using Adeotek.DevOpsTools.CommandsSettings.Containers;
using Adeotek.Extensions.Containers.Config;

namespace Adeotek.DevOpsTools.Commands.Containers;

internal sealed class ContainersActionCommand : ContainersBaseCommand<ContainersActionSettings>
{
    protected override string ResultLabel => "Changes";
    
    protected override void ExecuteContainerCommand(ContainersConfig config)
    {
        var containersManager = GetContainersManager();
        foreach (var service in GetTargetServices(config, _settings?.ServiceName))
        {
            if (!containersManager.ContainerExists(service.CurrentName))
            {
                PrintMessage($"<{service.ServiceName}> Container not found, unable to perform {_commandName} action!", _errorColor);
                continue;
            }
        
            PrintMessage($"<{service.ServiceName}> Executing {_commandName} command...");
            Changes += _commandName switch
            {
                "start" => containersManager.StartContainer(service.CurrentName, IsDryRun),
                "stop" => containersManager.StopContainer(service.CurrentName, IsDryRun),
                "restart" => containersManager.RestartService(service.CurrentName, IsDryRun),
                _ => throw new NotImplementedException($"Unknown command: {_commandName}")
            };

            PrintMessage($"<{service.ServiceName}> Command {_commandName} executed successfully.", _successColor, separator: IsVerbose);
            if (IsDryRun)
            {
                PrintMessage("Dry run: No changes were made!", _warningColor);
            }
        }
    }
}
