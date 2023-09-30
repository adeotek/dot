using Adeotek.DevOpsTools.Settings;
using Adeotek.Extensions.Docker.Config;

namespace Adeotek.DevOpsTools.Commands;

internal sealed class ContainerDownCommand : ContainerBaseCommand<ContainerDownSettings>
{
    private bool Purge => _settings?.Purge ?? false;
    
    protected override void ExecuteCommand(ContainerConfig config)
    {
        if (CheckIfContainerExists(config.PrimaryName))
        {
            PrintMessage("Container found, removing it.");
            RemoveContainer(config, Purge);
            return;
        }
        
        if (Purge)
        {
            PrintMessage("Container not found, trying to purge resources.", _warningColor);
            RemoveContainer(config, Purge);
        }
        else
        {
            PrintMessage("Container not fond, nothing to do!", _warningColor);    
        }
    }
}