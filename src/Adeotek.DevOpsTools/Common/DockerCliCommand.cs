using Adeotek.DevOpsTools.Models;
using Adeotek.Extensions.Processes;

namespace Adeotek.DevOpsTools.Common;

public class DockerCliCommand : ShellCommand
{
    public DockerCliCommand()
    {
        Command = "docker";
    }
    
    public DockerCliCommand AddPortArgs(PortMapping[] ports)
    {
        foreach (var port in ports)
        {
            AddArgument($"-p {port.Host}:{port.Container}");
        }
        
        return this;
    }
    
    public DockerCliCommand AddVolumeArgs(VolumeConfig[] volumes)
    {
        foreach (var volume in volumes)
        {
            AddArgument($"-v {volume.Source}:{volume.Destination}");
        }
        
        return this;
    }

    public DockerCliCommand AddEnvVarArgs(Dictionary<string, string> envVars)
    {
        foreach ((string name, string value) in envVars)
        {
            AddArgument(value.Contains('=') 
                ? $"-e {name}=\"{value}\"" 
                : $"-e {name}={value}");
        }
        
        return this;
    }
    
    public DockerCliCommand AddNetworkArgs(ContainerConfig config)
    {
        if (config.Network is null)
        {
            return this;
        }
        
        if (!string.IsNullOrEmpty(config.Network.Name))
        {
            AddArgument($"--network={config.Network.Name}");
        }
        if (!string.IsNullOrEmpty(config.Network.IpAddress))
        {
            AddArgument($"--ip={config.Network.IpAddress}");
        }
        if (config.Network.Hostname != "")
        {
            AddArgument($"--hostname={config.Network.Hostname ?? config.PrimaryName}");
        }
        if (config.Network.Alias != "")
        {
            AddArgument($"--network-alias={config.Network.Alias ?? config.BaseName}");
        }

        return this;
    }

    public DockerCliCommand AddRestartArg(string? restart)
    {
        if (restart == "")
        {
            return this;
        }
        
        AddArgument($"--restart={restart ?? "unless-stopped"}");

        return this;
    }

    public DockerCliCommand AddFilterArg(string value, string? key = null)
    {
        AddArgument($"--filter {key ?? "name"}={value}");
        return this;
    }
    
    public DockerCliCommand AddArg(string value)
    {
        AddArgument(value);
        return this;
    }
    
    public DockerCliCommand ClearArgs()
    {
        ClearArguments();
        return this;
    }
}