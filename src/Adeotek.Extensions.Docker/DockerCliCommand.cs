using Adeotek.Extensions.Docker.Config;
using Adeotek.Extensions.Processes;

namespace Adeotek.Extensions.Docker;

public class DockerCliCommand : ShellCommand
{
    public static DockerCliCommand GetDockerCliCommandInstance(
        OutputReceivedEventHandler? onStdOutput = null,
        OutputReceivedEventHandler? onErrOutput = null)
    {
        var instance = new DockerCliCommand(new DefaultShellProcessProvider()) { Command = "docker" };
        if (onStdOutput is not null)
        {
            instance.OnStdOutput += onStdOutput;
        }
        if (onErrOutput is not null)
        {
            instance.OnErrOutput += onErrOutput;
        }
        return instance;
    }

    public DockerCliCommand(IShellProcessProvider shellProcessProvider) 
        : base(shellProcessProvider)
    {
        Command = "docker";
    }
    
    public new DockerCliCommand AddArg(string value) =>
        (DockerCliCommand) base.AddArg(value);
    public new DockerCliCommand AddArg(IEnumerable<string> range) =>
        (DockerCliCommand) base.AddArg(range);
    public new DockerCliCommand SetArgAt(int index, string value) =>
        (DockerCliCommand) base.SetArgAt(index, value);
    public new DockerCliCommand ReplaceArg(string currentValue, string newValue) =>
        (DockerCliCommand) base.ReplaceArg(currentValue, newValue);
    public new DockerCliCommand RemoveArg(string item) =>
        (DockerCliCommand) base.RemoveArg(item);
    public new DockerCliCommand RemoveArgAt(int index) =>
        (DockerCliCommand) base.RemoveArgAt(index);
    public new DockerCliCommand ClearArgs() =>
        (DockerCliCommand) base.ClearArgs();
    public new DockerCliCommand ClearArgsAndReset() =>
        (DockerCliCommand) base.ClearArgsAndReset();
    
    public DockerCliCommand AddFilterArg(string value, string? key = null) => 
        AddArg($"--filter {key ?? "name"}={value}");
    
    public DockerCliCommand AddRestartArg(string? restart) => 
        restart == "" ? this : AddArg($"--restart={restart ?? "unless-stopped"}");

    public DockerCliCommand AddPortArg(uint hostPort, uint containerPort) =>
        AddArg($"-p {hostPort}:{containerPort}");
    
    public DockerCliCommand AddPortsArgs(PortMapping[] ports)
    {
        foreach (var port in ports)
        {
            AddPortArg(port.Host, port.Container);
        }
        return this;
    }
    
    public DockerCliCommand AddVolumeArg(string source, string destination, bool isReadonly = false) => 
        AddArg($"-v {source}:{destination}{(isReadonly ? ":ro" : "")}");
    
    public DockerCliCommand AddVolumesArgs(IEnumerable<VolumeConfig> volumes)
    {
        foreach (var volume in volumes)
        {
            AddVolumeArg(volume.Source, volume.Destination, volume.IsReadonly);
        }
        return this;
    }
    
    public DockerCliCommand AddEnvVarArg(string name, string value) => 
        AddArg(value.Contains('=') || value.Contains(' ') ? $"-e {name}=\"{value}\"" : $"-e {name}={value}");

    public DockerCliCommand AddEnvVarsArgs(Dictionary<string, string> envVars)
    {
        foreach ((string name, string value) in envVars)
        {
            AddEnvVarArg(name, value);
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
            AddArg($"--network={config.Network.Name}");
        }
        if (!string.IsNullOrEmpty(config.Network.IpAddress))
        {
            AddArg($"--ip={config.Network.IpAddress}");
        }
        if (config.Network.Hostname != "")
        {
            AddArg($"--hostname={config.Network.Hostname ?? config.CurrentName}");
        }
        if (config.Network.Alias != "")
        {
            AddArg($"--network-alias={config.Network.Alias ?? config.Name}");
        }

        return this;
    }
    
    public DockerCliCommand AddExtraHostArg(string name, string value) => 
        AddArg($"--add-host {name}:{value}");
    
    public DockerCliCommand AddExtraHostsArgs(Dictionary<string, string> envVars)
    {
        foreach ((string name, string value) in envVars)
        {
            AddExtraHostArg(name, value);
        }
        return this;
    }
}