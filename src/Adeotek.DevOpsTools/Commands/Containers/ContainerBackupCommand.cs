using Adeotek.DevOpsTools.CommandsSettings.Containers;
using Adeotek.Extensions.Docker.Config;

namespace Adeotek.DevOpsTools.Commands.Containers;

internal sealed class ContainerBackupCommand : ContainerBaseCommand<ContainerBackupSettings>
{
    protected override string CommandName => "container backup";
    protected override string ResultLabel => "Changes";
    
    protected override void ExecuteContainerCommand(ContainerConfigV1 configV1)
    {
        var dockerManager = GetDockerManager();
    }
}
