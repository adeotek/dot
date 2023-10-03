using Adeotek.DevOpsTools.CommandsSettings;
using Adeotek.Extensions.Docker;

using Spectre.Console.Cli;

namespace Adeotek.DevOpsTools.Commands;

internal class ContainerConfigValidateCommand : CommandBase<ContainerConfigValidateSettings>
{
    protected override int ExecuteCommand(CommandContext context, ContainerConfigValidateSettings settings)
    {
        var config = DockerConfigManager.LoadContainerConfig(settings.ConfigFile);
        bool hasErrors = false;
        
        if (string.IsNullOrEmpty(config.Image) || config.Image.Contains(' '))
        {
            hasErrors = true;
            PrintMessage("`Image` cannot be empty, nor contain any spaces!", _errorColor);
        }
        if (string.IsNullOrEmpty(config.BaseName) || config.BaseName.Contains(' '))
        {
            hasErrors = true;
            PrintMessage("`BaseName` cannot be empty, nor contain any spaces!", _errorColor);
        }

        if ((config.Tag?.Contains(' ') ?? false)
            || (config.NamePrefix?.Contains(' ') ?? false)
            || (config.PrimarySuffix?.Contains(' ') ?? false)
            || (config.BackupSuffix?.Contains(' ') ?? false))
        {
            hasErrors = true;
            PrintMessage("`Image` cannot be empty!", _errorColor);
        }
        
        if (config.Ports.Count(e => e.Host == 0 || e.Container == 0) > 0)
        {
            hasErrors = true;
            PrintMessage("`Ports[*].Host`/`Ports[*].Container` cannot be 0!", _errorColor);
        }

        if (config.Volumes.Count(e => string.IsNullOrEmpty(e.Source) || e.Source.Contains(' ')
            || string.IsNullOrEmpty(e.Destination) || e.Destination.Contains(' ')) > 0)
        {
            hasErrors = true;
            PrintMessage("`Volume[*].Source`/`Volume[*].Destination` cannot be empty, nor contain any spaces!", _errorColor);
        }
        
        if (config.Network is not null 
            && (string.IsNullOrEmpty(config.Network.Name) 
                || (config.Network?.Name.Contains(' ') ?? false)))
        {
            hasErrors = true;
            PrintMessage("`Network.Name` cannot be empty, nor contain any spaces!", _errorColor);
        }
        
        if (config.Restart is not null
            && config.Restart != "on-failure" 
            && config.Restart != "always"
            && config.Restart != "unless-stopped")
        {
            hasErrors = true;
            PrintMessage("`Restart` can only have one of the following values: null, 'always', 'on-failure' or 'unless-stopped'!", _errorColor);
        }

        if (hasErrors)
        {
            PrintMessage("The config file is not valid!", _warningColor, IsVerbose);    
            return 1;
        }

        PrintMessage("The config file is valid!", _successColor, IsVerbose);
        return 0;
    }
}