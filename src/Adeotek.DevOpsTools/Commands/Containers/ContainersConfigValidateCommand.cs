using Adeotek.DevOpsTools.CommandsSettings.Containers;
using Adeotek.Extensions.Docker;
using Adeotek.Extensions.Docker.Config;

using Spectre.Console.Cli;

namespace Adeotek.DevOpsTools.Commands.Containers;

internal class ContainersConfigValidateCommand : CommandBase<ContainersConfigValidateSettings>
{
    protected override string GetCommandName() => $"containers config {_commandName}";
    
    protected override int ExecuteCommand(CommandContext context, ContainersConfigValidateSettings settings)
    {
        var config = DockerConfigManager.LoadContainersConfig(settings.ConfigFile);
        bool hasErrors = false;

        if (config.Services.Count == 0)
        {
            hasErrors = true;
            PrintMessage("The `Services` list cannot be empty!", _errorColor);    
        }
        
        var serviceIndex = 0;
        foreach ((string serviceKey, ServiceConfig service) in config.Services)
        {
            if (string.IsNullOrWhiteSpace(serviceKey))
            {
                hasErrors = true;
                PrintMessage($"Service keys cannot be empty (network index: {serviceIndex})!", _errorColor);
            }
            
            if (string.IsNullOrEmpty(service.Image) || service.Image.Contains(' '))
            {
                hasErrors = true;
                PrintMessage($"`Image` cannot be empty, nor contain any spaces (service: {serviceKey})!", _errorColor);
            }

            if (!string.IsNullOrEmpty(service.PullPolicy) &&
                !ServiceConfig.PullPolicyValues.Contains(service.PullPolicy))
            {
                hasErrors = true;
                PrintMessage($"`PullPolicy` invalid value '{service.PullPolicy}'. The allowed values are: {string.Join('/', ServiceConfig.PullPolicyValues)} (service: {serviceKey})!", _errorColor);
            }
            
            if (string.IsNullOrEmpty(service.ContainerName) && string.IsNullOrEmpty(service.BaseName))
            {
                hasErrors = true;
                PrintMessage($"`ContainerName` and `BaseName` cannot be both empty (service: {serviceKey})!", _errorColor);
            }
            
            if (!string.IsNullOrEmpty(service.ContainerName) && service.ContainerName.Contains(' '))
            {
                hasErrors = true;
                PrintMessage($"`ContainerName` cannot contain any spaces (service: {serviceKey})!", _errorColor);
            }
            
            if (!string.IsNullOrEmpty(service.BaseName) && service.BaseName.Contains(' '))
            {
                hasErrors = true;
                PrintMessage($"`BaseName` cannot contain any spaces (service: {serviceKey})!", _errorColor);
            }
            
            if (!string.IsNullOrEmpty(service.BaseName) 
                && (service.NamePrefix?.Contains(' ') ?? false)
                && (service.CurrentSuffix?.Contains(' ') ?? false)
                && (service.PreviousSuffix?.Contains(' ') ?? false))
            {
                hasErrors = true;
                PrintMessage($"`NamePrefix`, `CurrentSuffix` and `PreviousSuffix` cannot contain any spaces (service: {serviceKey})!", _errorColor);
            }

            if (service.Networks is not null && service.Networks.Count > 0)
            {
                var serviceNetworkIndex = 0;
                foreach ((string? key, ServiceNetworkConfig? _) in service.Networks)
                {
                    if (string.IsNullOrEmpty(key))
                    {
                        hasErrors = true;
                        PrintMessage($"Service network key cannot be empty (service: {serviceKey} / network index: {serviceNetworkIndex})!", _errorColor);        
                    } 
                    else if (!config.Networks.Keys.Contains(key))
                    {
                        hasErrors = true;
                        PrintMessage($"Service network '{key}' not found in the `Networks` list (service: {serviceKey})!", _errorColor);
                    }

                    serviceNetworkIndex++;
                }
            }

            if (!string.IsNullOrEmpty(service.Restart) &&
                !ServiceConfig.RestartPolicyValues.Contains(service.Restart))
            {
                hasErrors = true;
                PrintMessage($"`Restart` invalid value '{service.Restart}'. The allowed values are: {string.Join('/', ServiceConfig.RestartPolicyValues)} (service: {serviceKey})!", _errorColor);
            }
            
            serviceIndex++;
        }

        var networkIndex = 0;
        foreach ((string networkKey, NetworkConfig network) in config.Networks)
        {
            if (string.IsNullOrWhiteSpace(networkKey))
            {
                hasErrors = true;
                PrintMessage($"Networks keys cannot be empty (network index: {networkIndex})!", _errorColor);
            }
                
            if (string.IsNullOrWhiteSpace(network.Name) || network.Name.Contains(' ')) 
            {
                hasErrors = true;
                PrintMessage($"Network `Name` cannot be empty, nor contain any spaces (network key: {networkKey}!", _errorColor);
            }

            networkIndex++;
        }

        if (hasErrors)
        {
            PrintMessage("The config file is not valid!", _warningColor, IsVerbose);    
            return 1;
        }

        PrintMessage("The config file is valid!", _successColor, IsVerbose);
        Changes++;
        return 0;
    }
}