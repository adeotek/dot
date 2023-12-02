using Adeotek.DevOpsTools.CommandsSettings.Containers;
using Adeotek.Extensions.Docker.Config;

namespace Adeotek.DevOpsTools.Commands.Containers;

internal sealed class ContainersActionCommand : ContainersBaseCommand<ContainersActionSettings>
{
    protected override string ResultLabel => "Changes";
    
    protected override void ExecuteContainerCommand(ContainersConfig config)
    {
        var dockerManager = GetDockerManager();
        foreach ((string name, ServiceConfig service) in GetTargetServices(config))
        {
            if (!dockerManager.ContainerExists(service.CurrentName))
            {
                PrintMessage($"<{name}> Container not found, unable to perform {_commandName} action!", _errorColor);
                return;
            }
        
            PrintMessage($"<{name}> Executing {_commandName}...");
            Changes += _commandName switch
            {
                "start" => dockerManager.StartContainer(service.CurrentName, IsDryRun),
                "stop" => dockerManager.StopContainer(service.CurrentName, IsDryRun),
                "restart" => dockerManager.RestartContainer(service.CurrentName, IsDryRun),
                _ => throw new NotImplementedException($"Unknown action: {_commandName}")
            };

            PrintMessage($"<{name}> Container {_commandName} done.", _standardColor, separator: IsVerbose);
            if (IsDryRun)
            {
                PrintMessage("Dry run: No changes were made!", _warningColor);
            }
        }
    }
}
