using Adeotek.DevOpsTools.Models;
using Adeotek.DevOpsTools.Settings;

namespace Adeotek.DevOpsTools.Commands;

internal sealed class ContainerUpCommand : ContainerBaseCommand<ContainerUpSettings>
{
    protected override void ExecuteCommand(ContainerConfig config)
    {
        if (CheckIfContainerExists(config.PrimaryName))
        {
            if (_settings is null || !_settings.Update)
            {
                PrintMessage("Container already present, nothing to do!", _standardColor);
                return;
            }
            
            PrintMessage("Container already present, updating it.", _warningColor);
            UpdateContainer(config);
            return;
        }

        PrintMessage("Container not fond, creating new one.", _standardColor);
        if (CreateContainer(config))
        {
            PrintMessage("Container created successfully!", _successColor);
        }
        else
        {
            PrintMessage("Command failed, the container was not created!", _errorColor);
        }
    }
}