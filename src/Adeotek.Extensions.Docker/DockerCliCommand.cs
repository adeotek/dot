using Adeotek.Extensions.Docker.Config;
using Adeotek.Extensions.Docker.Config.V1;
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

    public DockerCliCommand AddRunCommandOptionsArgs(string[] runCommandOptions) =>
        runCommandOptions.Length == 0 
            ? AddArg("-d")
            : AddArg(string.Join(' ', runCommandOptions).Trim());
    
    public DockerCliCommand AddStartupCommandArgs(ContainerConfigV1 configV1)
    {
        if (string.IsNullOrEmpty(configV1.Command))
        {
            return this;
        }

        AddArg(configV1.Command);
        if (configV1.CommandArgs.Length > 0)
        {
            AddArg(string.Join(' ', configV1.CommandArgs).Trim());
        }
        
        return this;
    }
    
    public DockerCliCommand AddRestartArg(string? restart) => 
        restart == "" ? this : AddArg($"--restart={restart ?? "unless-stopped"}");

    public DockerCliCommand AddPortArg(uint hostPort, uint containerPort) =>
        AddArg($"-p {hostPort}:{containerPort}");
    
    public DockerCliCommand AddPortsArgs(PortMappingV1[] ports)
    {
        foreach (var port in ports)
        {
            AddPortArg(port.Host, port.Container);
        }
        return this;
    }
    
    public DockerCliCommand AddVolumeArg(string source, string destination, bool isReadonly = false) => 
        AddArg($"-v {source}:{destination}{(isReadonly ? ":ro" : "")}");
    
    public DockerCliCommand AddVolumesArgs(IEnumerable<VolumeConfigV1> volumes)
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
    
    public DockerCliCommand AddNetworkArgs(ContainerConfigV1 configV1)
    {
        if (configV1.Network is null)
        {
            return this;
        }
        
        if (!string.IsNullOrEmpty(configV1.Network.Name))
        {
            AddArg($"--network={configV1.Network.Name}");
        }
        if (!string.IsNullOrEmpty(configV1.Network.IpAddress))
        {
            AddArg($"--ip={configV1.Network.IpAddress}");
        }
        if (configV1.Network.Hostname != "")
        {
            AddArg($"--hostname={configV1.Network.Hostname ?? configV1.CurrentName}");
        }
        if (configV1.Network.Alias != "")
        {
            AddArg($"--network-alias={configV1.Network.Alias ?? configV1.Name}");
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