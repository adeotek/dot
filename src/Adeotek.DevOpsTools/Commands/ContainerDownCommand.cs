using Adeotek.DevOpsTools.CommandsSettings;
using Adeotek.Extensions.Docker.Config;

namespace Adeotek.DevOpsTools.Commands;

internal sealed class ContainerDownCommand : ContainerBaseCommand<ContainerDownSettings>
{
    private bool Purge => _settings?.Purge ?? false;
    
    protected override void ExecuteContainerCommand(ContainerConfig config)
    {
        var dockerManager = GetDockerManager();
        if (dockerManager.ContainerExists(config.PrimaryName))
        {
            PrintMessage("Container found, removing it.");
            dockerManager.RemoveContainer(config, Purge);
            return;
        }
        
        if (Purge)
        {
            PrintMessage("Container not found, trying to purge resources.", _warningColor);
            dockerManager.RemoveContainer(config, Purge);
        }
        else
        {
            PrintMessage("Container not fond, nothing to do!", _warningColor);    
        }
    }
}