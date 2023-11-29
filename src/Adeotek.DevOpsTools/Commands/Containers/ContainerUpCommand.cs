using Adeotek.DevOpsTools.CommandsSettings.Containers;
using Adeotek.Extensions.Docker.Config;

namespace Adeotek.DevOpsTools.Commands.Containers;

internal sealed class ContainerUpCommand : ContainerBaseCommand<ContainerUpSettings>
{
    protected override string CommandName => "container up";
    protected override string ResultLabel => "Changes";
    private bool Upgrade => _settings?.Upgrade ?? false;
    private bool Replace => _settings?.Replace ?? false;
    private bool Force => _settings?.Force ?? false;
    
    protected override void ExecuteContainerCommand(ContainerConfigV1 configV1)
    {
        var dockerManager = GetDockerManager();
        if (dockerManager.ContainerExists(configV1.CurrentName))
        {
            if (!Upgrade)
            {
                PrintMessage("Container already present, nothing to do!", _successColor, separator: IsVerbose);
                return;
            }
        
            PrintMessage("Container already present, updating it.", _warningColor);
            Changes += dockerManager.UpgradeContainer(configV1, Replace, Force, IsDryRun);
            return;
        }
        
        PrintMessage("Container not fond, creating new one.");
        Changes += dockerManager.CheckAndCreateContainer(configV1, IsDryRun);
        
        if (Changes == 0)
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
