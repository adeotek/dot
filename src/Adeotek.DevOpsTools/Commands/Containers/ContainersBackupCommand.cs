using Adeotek.DevOpsTools.CommandsSettings.Containers;
using Adeotek.Extensions.Docker.Config;

namespace Adeotek.DevOpsTools.Commands.Containers;

internal sealed class ContainersBackupCommand : ContainersBaseCommand<ContainersBackupSettings>
{
    protected override string ResultLabel => "Changes";
    
    protected override void ExecuteContainerCommand(ContainersConfig config)
    {
        var dockerManager = GetDockerManager();
        
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
            
            Changes += dockerManager.BackupVolume(volume, _settings?.BackupLocation ?? "", IsDryRun);
        
            if (IsDryRun)
            {
                PrintMessage($"<{service?.ServiceName}> {_settings?.TargetVolume} volume backup finished.", _standardColor, separator: IsVerbose);
                PrintMessage("Dry run: No changes were made!", _warningColor);
            }
            else
            {
                PrintMessage($"<{service?.ServiceName}> {_settings?.TargetVolume} volume backup done!", _successColor, separator: IsVerbose);
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
            
            if (service.Volumes is null || service.Volumes.Length == 0)
            {
                PrintMessage($"<{service.ServiceName}> No volumes to backup!", _warningColor, separator: IsVerbose);
                continue;
            }

            Changes += service.Volumes
                .Sum(x => dockerManager.BackupVolume(x, _settings?.BackupLocation ?? "", IsDryRun));
        
            if (IsDryRun)
            {
                PrintMessage($"<{service.ServiceName}> Volumes backup finished.", _standardColor, separator: IsVerbose);
                PrintMessage("Dry run: No changes were made!", _warningColor);
            }
            else
            {
                PrintMessage($"<{service.ServiceName}> Volumes backup done!", _successColor, separator: IsVerbose);
            }
        }
    }
}
