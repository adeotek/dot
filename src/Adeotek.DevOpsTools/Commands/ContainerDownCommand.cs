﻿using Adeotek.DevOpsTools.CommandsSettings;
using Adeotek.Extensions.Docker.Config;

namespace Adeotek.DevOpsTools.Commands;

internal sealed class ContainerDownCommand : ContainerBaseCommand<ContainerDownSettings>
{
    private bool Purge => _settings?.Purge ?? false;
    
    protected override void ExecuteContainerCommand(ContainerConfig config)
    {
        var dockerManager = GetDockerManager();
        if (dockerManager.ContainerExists(config.PrimaryName))
        {
            PrintMessage("Container found, removing it.");
            dockerManager.RemoveContainer(config, Purge);
            PrintMessage($"Container removed{(Purge ? " and resources purged" : "")} successfully!", _successColor, separator: IsVerbose);
            return;
        }
        
        if (Purge)
        {
            PrintMessage("Container not found, trying to purge resources.", _warningColor);
            dockerManager.RemoveContainer(config, Purge);
            PrintMessage("Container resources purged successfully!", _successColor, separator: IsVerbose);
        }
        else
        {
            PrintMessage("Container not fond, nothing to do!", _warningColor);    
        }
    }
}