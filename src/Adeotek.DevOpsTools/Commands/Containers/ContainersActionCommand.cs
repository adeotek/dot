using Adeotek.DevOpsTools.CommandsSettings.Containers;
using Adeotek.Extensions.Docker.Config;

namespace Adeotek.DevOpsTools.Commands.Containers;

internal sealed class ContainersActionCommand : ContainersBaseCommand<ContainersActionSettings>
{
    protected override string ResultLabel => "Changes";
    
    protected override void ExecuteContainerCommand(ContainersConfig config)
    {
        var dockerManager = GetDockerManager();
        foreach (var service in GetTargetServices(config, _settings?.ServiceName))
        {
            if (!dockerManager.ContainerExists(service.CurrentName))
            {
                PrintMessage($"<{service.ServiceName}> Container not found, unable to perform {_commandName} action!", _errorColor);
                continue;
            }
        
            PrintMessage($"<{service.ServiceName}> Executing {_commandName} command...");
            Changes += _commandName switch
            {
                "start" => dockerManager.StartContainer(service.CurrentName, IsDryRun),
                "stop" => dockerManager.StopContainer(service.CurrentName, IsDryRun),
                "restart" => dockerManager.RestartService(service.CurrentName, IsDryRun),
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
