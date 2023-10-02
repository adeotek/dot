using Adeotek.DevOpsTools.CommandsSettings;
using Adeotek.Extensions.Docker;

using Spectre.Console.Cli;

namespace Adeotek.DevOpsTools.Commands;

internal class ContainerConfigValidateCommand : CommandBase<ContainerConfigSettings>
{
    protected override int ExecuteCommand(CommandContext context, ContainerConfigSettings settings)
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
        
        // TODO: add Volumes/Network/Restart validations

        if (hasErrors)
        {
            PrintMessage("The config file is not valid!", _warningColor, IsVerbose);    
            return 1;
        }

        PrintMessage("The config file is valid!", _successColor, IsVerbose);
        return 0;
    }
}