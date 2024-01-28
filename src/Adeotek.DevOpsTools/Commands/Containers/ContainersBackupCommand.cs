using Adeotek.DevOpsTools.CommandsSettings.Containers;
using Adeotek.Extensions.Containers.Config;

namespace Adeotek.DevOpsTools.Commands.Containers;

internal sealed class ContainersBackupCommand : ContainersBaseCommand<ContainersBackupSettings>
{
    protected override void ExecuteContainerCommand(ContainersConfig config)
    {
        var containersManager = GetContainersManager();
        
        var targetServices = GetTargetServices(config, _settings?.ServiceName);
        if (!string.IsNullOrEmpty(_settings?.TargetVolume))
        {
            var service = targetServices.FirstOrDefault();
            var volume = service?.Volumes?
                .FirstOrDefault(x => x.Source == _settings.TargetVolume);
            if (volume is null)
            {
                PrintMessage($"<{service?.ServiceName}> {_settings?.TargetVolume} volume not found!", _errorColor, separator: IsVerbose);
                return;
            }
            
            Changes += containersManager.BackupVolume(volume, _settings?.BackupLocation ?? "", out var backupFile, IsDryRun);
        
            if (IsDryRun)
            {
                PrintMessage($"<{service?.ServiceName}> {_settings?.TargetVolume} volume backup finished -> {backupFile}", _standardColor, separator: IsVerbose);
                PrintMessage("Dry run: No changes were made!", _warningColor);
            }
            else
            {
                PrintMessage($"<{service?.ServiceName}> {_settings?.TargetVolume} volume backup done -> {backupFile}", _successColor, separator: IsVerbose);
            }
            
            return;
        }
        
        var first = true;
        foreach (var service in targetServices)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                PrintSeparator();
            }

            BackupServiceVolumes(service, _settings?.BackupLocation, containersManager);
        }
    }
}
