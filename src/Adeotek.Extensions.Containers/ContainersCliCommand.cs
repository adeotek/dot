using System.Text;

using Adeotek.Extensions.Containers.Config;
using Adeotek.Extensions.Containers.Exceptions;
using Adeotek.Extensions.Processes;

namespace Adeotek.Extensions.Containers;

public class ContainersCliCommand : ShellCommand
{
    public static ContainersCliCommand GetContainersCliCommandInstance(
        OutputReceivedEventHandler? onStdOutput = null,
        OutputReceivedEventHandler? onErrOutput = null,
        string command = "docker")
    {
        var instance = new ContainersCliCommand(new DefaultShellProcessProvider()) { Command = command };
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

    public ContainersCliCommand(IShellProcessProvider shellProcessProvider, string command = "docker") 
        : base(shellProcessProvider)
    {
        Command = command;
    }

    public bool IsDocker => Command == "docker";
    
    public new ContainersCliCommand AddArg(string value) =>
        (ContainersCliCommand) base.AddArg(value);
    public new ContainersCliCommand AddArg(IEnumerable<string> range) =>
        (ContainersCliCommand) base.AddArg(range);
    public new ContainersCliCommand AddArgIf(string value, bool condition) =>
        condition ? (ContainersCliCommand) base.AddArg(value) : this;
    public new ContainersCliCommand SetArgAt(int index, string value) =>
        (ContainersCliCommand) base.SetArgAt(index, value);
    public new ContainersCliCommand ReplaceArg(string currentValue, string newValue) =>
        (ContainersCliCommand) base.ReplaceArg(currentValue, newValue);
    public new ContainersCliCommand RemoveArg(string item) =>
        (ContainersCliCommand) base.RemoveArg(item);
    public new ContainersCliCommand RemoveArgAt(int index) =>
        (ContainersCliCommand) base.RemoveArgAt(index);
    public new ContainersCliCommand ClearArgs() =>
        (ContainersCliCommand) base.ClearArgs();
    public new ContainersCliCommand ClearArgsAndReset() =>
        (ContainersCliCommand) base.ClearArgsAndReset();
    
    public ContainersCliCommand AddFilterArg(string value, string? key = null) => 
        AddArg($"--filter {key ?? "name"}={value}");

    public ContainersCliCommand AddInitCliOptionsArgs(ServiceConfig serviceConfig, bool isRun = true)
    {
        List<string> options = new();

        if (serviceConfig.Privileged)
        {
            options.Add("--privileged");
        }
        
        if (serviceConfig.InitCliOptions is not null && serviceConfig.InitCliOptions.Value.Value.Length > 0)
        {
            options.AddRange(isRun
                ? serviceConfig.InitCliOptions.Value.Value
                : serviceConfig.InitCliOptions.Value.Value.Where(x => x != "-d"));
        }
        else if (isRun)
        {
            options.Add("-d");
        }
        
        if (Command == "docker" &&
            serviceConfig.DockerCliOptions is not null && serviceConfig.DockerCliOptions.Value.Value.Length > 0)
        {
            options.AddRange(serviceConfig.DockerCliOptions.Value.Value);
        }
        else if (Command != "docker" && 
            serviceConfig.PodmanCliOptions is not null && serviceConfig.PodmanCliOptions.Value.Value.Length > 0)
        {
            options.AddRange(serviceConfig.PodmanCliOptions.Value.Value);
        }
            
        return options.Count > 0 ? AddArg(string.Join(' ', options).Trim()) : this;
    }

    public ContainersCliCommand AddStartupCommandArgs(ServiceConfig serviceConfig)
    {
        AddArgIf($"--entrypoint {serviceConfig.Entrypoint}", !string.IsNullOrEmpty(serviceConfig.Entrypoint));
        
        if (serviceConfig.Command is null || serviceConfig.Command.Value.Value.Length == 0)
        {
            return this;
        }

        AddArg(string.Join(' ', serviceConfig.Command.Value.AsArray()).Trim());
        return this;
    }
    
    public ContainersCliCommand AddRestartArg(string? restart) => 
        restart  == "" ? this : AddArg($"--restart={restart ?? "unless-stopped"}");
    
    public ContainersCliCommand AddPullPolicyArg(string? pullPolicy) => 
        pullPolicy == "" ? this : AddArg($"--pull={pullPolicy ?? "missing"}");
    
    public ContainersCliCommand AddPortArg(string? hostPort, string containerPort, string? hostIp = null, string? protocol = null)
    {
        StringBuilder sb = new();
        sb.Append("-p ");
        if (!string.IsNullOrEmpty(hostIp)) sb.Append($"{hostIp}:");
        if (!string.IsNullOrEmpty(hostIp) || !string.IsNullOrEmpty(hostPort)) sb.Append($"{hostPort ?? containerPort}:");
        sb.Append(containerPort);
        if (!string.IsNullOrEmpty(protocol)) sb.Append($"/{protocol}");
        return AddArg(sb.ToString());
    }
    
    public ContainersCliCommand AddPortsArgs(PortMapping[]? ports)
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

    public ContainersCliCommand AddExposedPortArgs(string? port) =>
        string.IsNullOrEmpty(port) ? this : AddArg($"--expose {port}");
    
    public ContainersCliCommand AddExposedPortsArgs(string[]? ports)
    {
        if (ports is null || ports.Length == 0)
        {
            return this;
        }
        
        foreach (var port in ports)
        {
            AddExposedPortArgs(port);
        }
        return this;
    }
    
    public ContainersCliCommand AddVolumeArg(string source, string target, bool isReadonly = false) => 
        AddArg($"-v {source}:{target}{(isReadonly ? ":ro" : "")}");
    
    public ContainersCliCommand AddVolumesArgs(VolumeConfig[]? volumes)
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
    
    public ContainersCliCommand AddEnvFilesArgs(string[]? envFiles)
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
    
    public ContainersCliCommand AddEnvVarArg(string name, string value) => 
        AddArg(value.Contains('=') || value.Contains(' ') ? $"-e {name}=\"{value}\"" : $"-e {name}={value}");

    public ContainersCliCommand AddEnvVarsArgs(Dictionary<string, string>? envVars)
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

    public ContainersCliCommand AddServiceNetworkArgs(ServiceNetworkConfig? serviceNetworkConfig)
    {
        AddArgIf($"--ip={serviceNetworkConfig?.IpV4Address}", !string.IsNullOrEmpty(serviceNetworkConfig?.IpV4Address));
        AddArgIf($"--ip6={serviceNetworkConfig?.IpV6Address}", !string.IsNullOrEmpty(serviceNetworkConfig?.IpV6Address));
        
        if (serviceNetworkConfig?.Aliases is null || serviceNetworkConfig.Aliases.Length <= 0)
        {
            return this;
        }

        foreach (var alias in serviceNetworkConfig.Aliases)
        {
            AddArgIf($"--network-alias={alias}", !string.IsNullOrEmpty(alias));
        }
        
        return this;
    }
    
    public ContainersCliCommand AddDefaultNetworkArgs(ServiceConfig serviceConfig, List<NetworkConfig>? networks)
    {
        if (serviceConfig.Networks is null || serviceConfig.Networks.Count == 0)
        {
            return this;
        }

        var serviceNetwork = serviceConfig.Networks.First();
        var network = networks?.FirstOrDefault(x => x.NetworkName == serviceNetwork.Key);
        ContainersCliException.ThrowIfNull(network, "run", $"Undefined network: `{serviceNetwork.Key}`");
        AddArg($"--network={network.Name}");
        AddArgIf($"--hostname={serviceConfig.Hostname ?? serviceConfig.CurrentName}", serviceConfig.Hostname != "");
        return AddServiceNetworkArgs(serviceNetwork.Value);
    }
    
    public ContainersCliCommand AddExtraHostArg(string name, string value) => 
        AddArg($"--add-host {name}:{value}");
    
    public ContainersCliCommand AddExtraHostsArgs(Dictionary<string, string>? envVars)
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
    
    public ContainersCliCommand AddLinksArgs(string[]? links)
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
    
    public ContainersCliCommand AddDnsArgs(string[]? dns)
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
    
    public ContainersCliCommand AddLabelsArgs(Dictionary<string, string>? labels)
    {
        if (labels is null || labels.Count == 0)
        {
            return this;
        }
        
        foreach ((string name, string value) in labels)
        {
            AddArg($"-l \"{name}\"=\"{value}\"");
        }
        return this;
    }
}