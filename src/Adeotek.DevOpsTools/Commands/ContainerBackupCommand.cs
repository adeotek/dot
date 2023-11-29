using Adeotek.DevOpsTools.Commands.Containers;
using Adeotek.DevOpsTools.CommandsSettings;
using Adeotek.Extensions.Docker.Config;

namespace Adeotek.DevOpsTools.Commands;

internal sealed class ContainerBackupCommand : ContainerBaseCommand<ContainerBackupSettings>
{
    
    protected override void ExecuteContainerCommand(ContainerConfigV1 configV1)
    {
        var dockerManager = GetDockerManager();
        
    }

    protected override string CommandName { get; }
}
