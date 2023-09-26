using Adeotek.DevOpsTools.Models;
using Adeotek.DevOpsTools.Settings;

namespace Adeotek.DevOpsTools.Commands;

internal sealed class ContainerDownCommand : ContainerBaseCommand<ContainerDownSettings>
{
    protected override void ExecuteCommand(ContainerConfig config)
    {
        if (CheckIfContainerExists(config.PrimaryName))
        {
            PrintMessage("Container found, removing it.", _standardColor);
            RemoveContainer(config);
        }
        else
        {
            PrintMessage("Container not fond, nothing to do!", _warningColor);
        }
    }
}