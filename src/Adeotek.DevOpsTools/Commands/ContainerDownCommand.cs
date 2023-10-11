using Adeotek.DevOpsTools.CommandsSettings;
using Adeotek.Extensions.Docker.Config;

namespace Adeotek.DevOpsTools.Commands;

internal sealed class ContainerDownCommand : ContainerBaseCommand<ContainerDownSettings>
{
    private bool Downgrade => _settings?.Downgrade ?? false;
    private bool Purge => _settings?.Purge ?? false;
    
    protected override void ExecuteContainerCommand(ContainerConfig config)
    {
        var dockerManager = GetDockerManager();
        if (Downgrade)
        {
            if (!dockerManager.ContainerExists(config.BackupName))
            {
                PrintMessage($"Backup container '{config.BackupName}' not fond, rollback not possible!", _warningColor);
                return;
            }

            PrintMessage("Backup container found, downgrading.");
            MadeChanges = dockerManager.DowngradeContainer(config, IsDryRun);
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
        
        if (dockerManager.ContainerExists(config.PrimaryName))
        {
            PrintMessage("Container found, removing it.");
            MadeChanges = dockerManager.PurgeContainer(config, Purge, IsDryRun);
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
            MadeChanges = dockerManager.PurgeContainer(config, Purge, IsDryRun);
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