using Adeotek.DevOpsTools.CommandsSettings.Containers;
using Adeotek.Extensions.Docker.Config;

namespace Adeotek.DevOpsTools.Commands.Containers;

internal sealed class ContainerBackupCommand : ContainerBaseCommand<ContainerBackupSettings>
{
    protected override string CommandName => "containers backup";
    protected override string ResultLabel => "Changes";
    
    protected override void ExecuteContainerCommand(ContainersConfig configV1)
    {
        var dockerManager = GetDockerManager();
    }
}
