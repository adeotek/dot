using Adeotek.DevOpsTools.CommandsSettings;
using Adeotek.Extensions.Docker.Config;

namespace Adeotek.DevOpsTools.Commands;

internal sealed class ContainerUpCommand : ContainerBaseCommand<ContainerUpSettings>
{
    private bool Update => _settings?.Update ?? false;
    private bool Replace => _settings?.Replace ?? false;
    
    protected override void ExecuteContainerCommand(ContainerConfig config)
    {
        var dockerManager = GetDockerManager();
        if (dockerManager.ContainerExists(config.PrimaryName))
        {
            if (!Update)
            {
                PrintMessage("Container already present, nothing to do!");
                return;
            }
        
            PrintMessage("Container already present, updating it.", _warningColor);
            dockerManager.UpdateContainer(config, Replace);
            return;
        }
        
        PrintMessage("Container not fond, creating new one.");
        if (dockerManager.CreateContainer(config))
        {
            PrintMessage("Container created successfully!", _successColor);
        }
        else
        {
            PrintMessage("Command failed, the container was not created!", _errorColor);
        }
    }
}
