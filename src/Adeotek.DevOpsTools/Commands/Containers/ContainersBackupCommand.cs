using Adeotek.DevOpsTools.CommandsSettings.Containers;
using Adeotek.Extensions.Docker.Config;

namespace Adeotek.DevOpsTools.Commands.Containers;

internal sealed class ContainersBackupCommand : ContainersBaseCommand<ContainersBackupSettings>
{
    protected override string ResultLabel => "Changes";
    
    protected override void ExecuteContainerCommand(ContainersConfig configV1)
    {
        var dockerManager = GetDockerManager();
    }
}
