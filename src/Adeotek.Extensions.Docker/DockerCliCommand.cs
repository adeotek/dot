using System.Text;

using Adeotek.Extensions.Docker.Config;
using Adeotek.Extensions.Docker.Exceptions;
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
    public new DockerCliCommand AddArgIf(string value, bool condition) =>
        condition ? (DockerCliCommand) base.AddArg(value) : this;
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

    public DockerCliCommand AddRunCommandOptionsArgs(string[]? runCommandOptions) =>
        runCommandOptions is null || runCommandOptions.Length == 0 
            ? AddArg("-d")
            : AddArg(string.Join(' ', runCommandOptions).Trim());
    
    public DockerCliCommand AddStartupCommandArgs(ServiceConfig serviceConfig)
    {
        AddArgIf($"--entrypoint {serviceConfig.Entrypoint}", !string.IsNullOrEmpty(serviceConfig.Entrypoint));
        
        if (serviceConfig.Command is null || serviceConfig.Command.Length == 0)
        {
            return this;
        }

        AddArg(string.Join(' ', serviceConfig.Command).Trim());
        return this;
    }
    
    public DockerCliCommand AddRestartArg(string? restart) => 
        restart  == "" ? this : AddArg($"--restart={restart ?? "unless-stopped"}");
    
    public DockerCliCommand AddPullPolicyArg(string? pullPolicy) => 
        pullPolicy == "" ? this : AddArg($"--pull={pullPolicy ?? "missing"}");
    
    public DockerCliCommand AddPortArg(string? hostPort, string containerPort, string? hostIp = null, string? protocol = null)
    {
        StringBuilder sb = new();
        sb.Append("-p ");
        if (string.IsNullOrEmpty(hostIp)) sb.Append($"{hostIp}:");
        if (string.IsNullOrEmpty(hostIp) || string.IsNullOrEmpty(hostPort)) sb.Append($"{hostPort ?? containerPort}:");
        sb.Append(containerPort);
        if (string.IsNullOrEmpty(protocol)) sb.Append($"/{protocol}");
        return AddArg(sb.ToString());
    }
    
    public DockerCliCommand AddPortsArgs(PortMapping[]? ports)
    {
        if (ports is null || ports.Length == 0)
        {
            return this;
        }
        
        foreach (var port in ports)
        {
            AddPortArg(port.Published, port.Target, port.HostIp, port.Protocol);
        }
        return this;
    }
    
    public DockerCliCommand AddVolumeArg(string source, string target, bool isReadonly = false) => 
        AddArg($"-v {source}:{target}{(isReadonly ? ":ro" : "")}");
    
    public DockerCliCommand AddVolumesArgs(VolumeConfig[]? volumes)
    {
        if (volumes is null || volumes.Length == 0)
        {
            return this;
        }
        
        foreach (var volume in volumes)
        {
            AddVolumeArg(volume.Source, volume.Target, volume.ReadOnly);
        }
        return this;
    }
    
    public DockerCliCommand AddEnvFilesArgs(string[]? envFiles)
    {
        if (envFiles is null || envFiles.Length == 0)
        {
            return this;
        }
        
        foreach (var file in envFiles)
        {
            AddArg($"--env-file {file}");
        }
        return this;
    }
    
    public DockerCliCommand AddEnvVarArg(string name, string value) => 
        AddArg(value.Contains('=') || value.Contains(' ') ? $"-e {name}=\"{value}\"" : $"-e {name}={value}");

    public DockerCliCommand AddEnvVarsArgs(Dictionary<string, string>? envVars)
    {
        if (envVars is null || envVars.Count == 0)
        {
            return this;
        }
        
        foreach ((string name, string value) in envVars)
        {
            AddEnvVarArg(name, value);
        }
        return this;
    }
    
    public DockerCliCommand AddNetworkArgs(ServiceConfig serviceConfig, List<NetworkConfig>? networks)
    {
        if (serviceConfig.Networks is null || serviceConfig.Networks.Count == 0)
        {
            return this;
        }

        foreach ((string name, ServiceNetworkConfig serviceNetwork) in serviceConfig.Networks)
        {
            var network = networks?.FirstOrDefault(x => x.NetworkName == name);
            DockerCliException.ThrowIfNull(network, "run", $"Undefined network: `{name}`");
            AddArg($"--network={network.Name}");
            AddArgIf($"--ip={serviceNetwork.IpV4Address}", !string.IsNullOrEmpty(serviceNetwork.IpV4Address));

            if (serviceNetwork.Aliases is null || serviceNetwork.Aliases.Length == 0)
            {
                continue;
            }

            foreach (var alias in serviceNetwork.Aliases)
            {
                AddArgIf($"--network-alias={alias}", !string.IsNullOrEmpty(alias));
            }
        }
        
        return AddArgIf($"--hostname={serviceConfig.Hostname ?? serviceConfig.CurrentName}", serviceConfig.Hostname != "");
    }
    
    public DockerCliCommand AddExtraHostArg(string name, string value) => 
        AddArg($"--add-host {name}:{value}");
    
    public DockerCliCommand AddExtraHostsArgs(Dictionary<string, string>? envVars)
    {
        if (envVars is null || envVars.Count == 0)
        {
            return this;
        }
        
        foreach ((string name, string value) in envVars)
        {
            AddExtraHostArg(name, value);
        }
        return this;
    }
    
    public DockerCliCommand AddLinksArgs(string[]? links)
    {
        if (links is null || links.Length == 0)
        {
            return this;
        }
        
        foreach (var link in links)
        {
            AddArg($"--link {link}");
        }
        return this;
    }
    
    public DockerCliCommand AddDnsArgs(string[]? dns)
    {
        if (dns is null || dns.Length == 0)
        {
            return this;
        }
        
        foreach (var entry in dns)
        {
            AddArg($"--dns {entry}");
        }
        return this;
    }
}