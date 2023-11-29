using Adeotek.DevOpsTools.CommandsSettings.Containers;
using Adeotek.Extensions.Docker.Config;

namespace Adeotek.DevOpsTools.Commands.Containers;

internal sealed class ContainerDownCommand : ContainerBaseCommand<ContainerDownSettings>
{
    protected override string CommandName => "container down";
    protected override string ResultLabel => "Changes";
    private bool Downgrade => _settings?.Downgrade ?? false;
    private bool Purge => _settings?.Purge ?? false;
    
    protected override void ExecuteContainerCommand(ContainerConfigV1 configV1)
    {
        var dockerManager = GetDockerManager();
        if (Downgrade)
        {
            if (!dockerManager.ContainerExists(configV1.PreviousName))
            {
                PrintMessage($"Previous container '{configV1.PreviousName}' not fond, rollback not possible!", _warningColor);
                return;
            }

            PrintMessage("Previous container found, downgrading.");
            Changes += dockerManager.DowngradeContainer(configV1, IsDryRun);
            if (IsDryRun)
            {
                PrintMessage("Container downgrade finished.", _standardColor, separator: IsVerbose);
                PrintMessage("Dry run: No changes were made!", _warningColor);
            }
            else
            {
                PrintMessage("Container downgrading successfully!", _successColor, separator: IsVerbose);
            }
            return;
        }
        
        if (dockerManager.ContainerExists(configV1.CurrentName))
        {
            PrintMessage("Container found, removing it.");
            Changes += dockerManager.PurgeContainer(configV1, Purge, IsDryRun);
            if (IsDryRun)
            {
                PrintMessage("Container remove finished.", _standardColor, separator: IsVerbose);
                PrintMessage("Dry run: No changes were made!", _warningColor);
            }
            else
            {
                PrintMessage($"Container removed{(Purge ? " and resources purged" : "")} successfully!", _successColor, separator: IsVerbose);
            }
            return;
        }
        
        if (Purge)
        {
            PrintMessage("Container not found, trying to purge resources.", _warningColor);
            Changes += dockerManager.PurgeContainer(configV1, Purge, IsDryRun);
            if (IsDryRun)
            {
                PrintMessage("Container resources purge finished.", _standardColor, separator: IsVerbose);
                PrintMessage("Dry run: No changes were made!", _warningColor);
            }
            else
            {
                PrintMessage("Container resources purged successfully!", _successColor, separator: IsVerbose);
            }
        }
        else
        {
            PrintMessage("Container not fond, nothing to do!", _warningColor);    
        }
    }
}