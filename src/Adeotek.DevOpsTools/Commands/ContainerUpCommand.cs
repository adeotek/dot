using Adeotek.DevOpsTools.CommandsSettings;
using Adeotek.Extensions.Docker.Config;

namespace Adeotek.DevOpsTools.Commands;

internal sealed class ContainerUpCommand : ContainerBaseCommand<ContainerUpSettings>
{
    private bool Upgrade => _settings?.Upgrade ?? false;
    private bool Replace => _settings?.Replace ?? false;
    private bool Force => _settings?.Force ?? false;
    
    protected override void ExecuteContainerCommand(ContainerConfig config)
    {
        var dockerManager = GetDockerManager();
        if (dockerManager.ContainerExists(config.PrimaryName))
        {
            if (!Upgrade)
            {
                PrintMessage("Container already present, nothing to do!", _successColor, separator: IsVerbose);
                return;
            }
        
            PrintMessage("Container already present, updating it.", _warningColor);
            MadeChanges = dockerManager.UpgradeContainer(config, Replace, Force, IsDryRun);
            return;
        }
        
        PrintMessage("Container not fond, creating new one.");
        MadeChanges = dockerManager.CheckAndCreateContainer(config, IsDryRun);
        
        if (!MadeChanges)
        {
            PrintMessage("Command failed, the container was not created!", _errorColor);
            return;
        }
        
        if (IsDryRun)
        {
            PrintMessage("Container create finished.", _standardColor, separator: IsVerbose);
            PrintMessage("Dry run: No changes were made!", _warningColor);
        }
        else
        {
            PrintMessage("Container created successfully!", _successColor, separator: IsVerbose);
        }
    }
}
