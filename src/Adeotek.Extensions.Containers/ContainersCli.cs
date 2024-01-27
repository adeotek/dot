using Adeotek.Extensions.Containers.Config;
using Adeotek.Extensions.Containers.Exceptions;
using Adeotek.Extensions.Processes;

namespace Adeotek.Extensions.Containers;

public class ContainersCli
{
    public delegate void ContainersCliEventHandler(object sender, ContainersCliEventArgs e);
    public event ContainersCliEventHandler? OnContainersCliEvent;
    
    private readonly ContainersCliCommand _containersCli;
    
    public ContainersCli(string cliFlavor = "docker", ContainersCliCommand? containersCli = null)
    {
        _containersCli = containersCli ?? ContainersCliCommand.GetContainersCliCommandInstance(
            CommandStdOutputHandler, CommandErrOutputHandler);
        _containersCli.Command = cliFlavor;
    }
    
    public string[] LastStdOutput => _containersCli.StdOutput.ToArray();
    public string[] LastErrOutput => _containersCli.ErrOutput.ToArray();
    public int LastStatusCode => _containersCli.ExitCode;
    
    public bool ContainerExists(string name)
    {
        _containersCli.ClearArgsAndReset()
            .AddArg("container")
            .AddArg("inspect")
            .AddArg("--format \"{{lower .Name}}\"")
            .AddArg(name);
        LogCommand();
        _containersCli.Execute();
        LogExitCode();
        if (_containersCli.IsSuccess(name))
        {
            return true;
        }

        if (_containersCli.IsError(name))
        {
            return false;
        }
        
        throw new ContainersCliException("container inspect", 1, $"Unable to inspect container '{name}'!");
    }
    
    public int CreateContainer(ServiceConfig serviceConfig, List<NetworkConfig>? networks, bool autoStart, bool dryRun = false)
    {
        var isRun = autoStart && (serviceConfig.Networks?.Count ?? 0) < 2;
        _containersCli.ClearArgsAndReset()
            .AddArg(isRun ? "run" : "create")
            .AddInitCliOptionsArgs(serviceConfig, isRun)
            .AddArg($"--name={serviceConfig.CurrentName}")
            .AddPortsArgs(serviceConfig.Ports)
            .AddVolumesArgs(serviceConfig.Volumes)
            .AddEnvFilesArgs(serviceConfig.EnvFiles)
            .AddEnvVarsArgs(serviceConfig.EnvVars)
            .AddDefaultNetworkArgs(serviceConfig, networks)
            .AddLinksArgs(serviceConfig.Links)
            .AddExtraHostsArgs(serviceConfig.ExtraHosts)
            .AddDnsArgs(serviceConfig.Dns)
            .AddExposedPortsArgs(serviceConfig.Expose)
            .AddRestartArg(serviceConfig.Restart)
            .AddPullPolicyArg(serviceConfig.PullPolicy)
            .AddArg(serviceConfig.Image)
            .AddStartupCommandArgs(serviceConfig);
        
        LogCommand();
        if (dryRun)
        {
            return 0;
        }
        _containersCli.Execute();
        LogExitCode();
        if (_containersCli is { ExitCode: 0, StdOutput.Count: 1 }
            && !string.IsNullOrEmpty(_containersCli.StdOutput.FirstOrDefault()))
        {
            return 1;
        }

        if (_containersCli.IsError(serviceConfig.CurrentName))
        {
            return 0;
        }
        
        throw new ContainersCliException("run", 1, $"Unable to create container '{serviceConfig.CurrentName}'!");
    }
    
    public int AttachContainerToNetwork(string containerName, string networkName, ServiceNetworkConfig? serviceNetworkConfig, bool dryRun = false)
    {
        _containersCli.ClearArgsAndReset()
            .AddArg("network connect")
            .AddServiceNetworkArgs(serviceNetworkConfig)
            .AddArg(networkName)
            .AddArg(containerName);
        LogCommand();
        if (dryRun)
        {
            return 0;
        }
        _containersCli.Execute();
        LogExitCode();
        if (_containersCli.IsSuccess())
        {
            return 1;
        }

        if (_containersCli.IsError($"Error response from daemon: endpoint with name {containerName} already exists in network {networkName}"))
        {
            LogMessage($"Container '{containerName}' already attached to network '{networkName}'!", "warn");
            return 0;    
        }

        if (_containersCli.IsError($"Error response from daemon: No such container: {containerName}", true))
        {
            throw new ContainersCliException("network connect", 1, $"Container '{containerName}' not found!");    
        }

        throw new ContainersCliException("network connect", 1, $"Unable to attach container '{containerName}' to network '{networkName}'!");
    }
    
    public int StartContainer(string containerName, bool dryRun = false)
    {
        _containersCli.ClearArgsAndReset()
            .AddArg("start")
            .AddArg(containerName);
        LogCommand();
        if (dryRun)
        {
            return 0;
        }
        _containersCli.Execute();
        LogExitCode();
        if (_containersCli.IsSuccess(containerName, true))
        {
            return 1;
        }
        
        if (!_containersCli.IsError($"Error response from daemon: No such container: {containerName}", true))
        {
            throw new ContainersCliException("start", 1, $"Unable to stop container '{containerName}'!");    
        }
            
        LogMessage($"Container '{containerName}' not found!", "warn");
        return 0;
    }
    
    public int StopContainer(string containerName, bool dryRun = false)
    {
        _containersCli.ClearArgsAndReset()
            .AddArg("stop")
            .AddArg(containerName);
        LogCommand();
        if (dryRun)
        {
            return 0;
        }
        _containersCli.Execute();
        LogExitCode();
        if (_containersCli.IsSuccess(containerName, true))
        {
            return 1;
        }
        
        if (!_containersCli.IsError($"Error response from daemon: No such container: {containerName}", true))
        {
            throw new ContainersCliException("stop", 1, $"Unable to stop container '{containerName}'!");    
        }
            
        LogMessage($"Container '{containerName}' not found!", "warn");
        return 0;
    }
    
    public int RemoveContainer(string containerName, bool dryRun = false)
    {
        _containersCli.ClearArgsAndReset()
            .AddArg("rm")
            .AddArg(containerName);
        LogCommand();
        if (dryRun)
        {
            return 0;
        }
        _containersCli.Execute();
        LogExitCode();
        if (_containersCli.IsSuccess(containerName, true))
        {
            return 1;
        }

        if (!_containersCli.IsError($"Error response from daemon: No such container: {containerName}", true))
        {
            throw new ContainersCliException("rm", 1, $"Unable to remove container '{containerName}'!");
        }

        LogMessage($"Container '{containerName}' not found!", "warn");
        return 0;
    }
    
    public int RenameContainer(string currentName, string newName, bool dryRun = false)
    {
        _containersCli.ClearArgsAndReset()
            .AddArg("rename")
            .AddArg(currentName)
            .AddArg(newName);
        LogCommand();
        if (dryRun)
        {
            return 0;
        }
        _containersCli.Execute();
        LogExitCode();
        if (_containersCli.IsSuccess())
        {
            return 1;
        }

        if (!_containersCli.IsError($"Error response from daemon: No such container: {currentName}", true))
        {
            throw new ContainersCliException("rename", 1, $"Unable to rename container '{currentName}'!");
        }

        LogMessage($"Container '{currentName}' not found!", "warn");
        return 0;
    }

    public bool VolumeExists(string volumeName)
    {
        _containersCli.ClearArgsAndReset()
            .AddArg("volume")
            .AddArg("ls")
            .AddFilterArg(volumeName);
        LogCommand();
        _containersCli.Execute();
        LogExitCode();
        return _containersCli.IsSuccess(volumeName);
    }

    public int CreateVolume(string volumeName, bool dryRun = false)
    {
        _containersCli.ClearArgsAndReset()
            .AddArg("volume")
            .AddArg("create")
            .AddArg(volumeName);
        LogCommand();
        if (dryRun)
        {
            return 0;
        }
        
        _containersCli.Execute();
        LogExitCode();
        if (!_containersCli.IsSuccess(volumeName, true))
        {
            throw new ContainersCliException("volume create", 1, $"Unable to create volume '{volumeName}'!");
        }

        return 1;
    }

    public int CreateBindVolume(VolumeConfig volume, bool dryRun = false)
    {
        var changes = 0;
        LogCommand("mkdir", volume.Source);
        if (!dryRun)
        {
            Directory.CreateDirectory(volume.Source);
            changes++;
        }
        
        if (ShellCommand.IsWindowsPlatform)
        {
            return changes;
        }

        var bashCommand = ShellCommand.GetShellCommandInstance(
                shell: ShellCommand.BashShell,
                command: "chgrp",
                onStdOutput: CommandStdOutputHandler,
                onErrOutput: CommandErrOutputHandler)
            .AddArg("docker")
            .AddArg(volume.Source);
        LogCommand(bashCommand.ProcessFile, bashCommand.ProcessArguments);
        if (dryRun)
        {
            return 0;
        }
        bashCommand.Execute();
        LogExitCode(bashCommand.ExitCode, bashCommand.ProcessFile, bashCommand.ProcessArguments);
        if (!bashCommand.IsSuccess())
        {
            throw new ShellCommandException(1, $"Unable to set group 'docker' for '{volume.Source}' directory!");
        }

        return 1;
    }
    
    public int RemoveVolume(string volumeName, bool dryRun = false)
    {
        _containersCli.ClearArgsAndReset()
            .AddArg("volume")
            .AddArg("rm")
            .AddArg(volumeName);
        LogCommand();
        if (dryRun)
        {
            return 0;
        }
        _containersCli.Execute();
        LogExitCode();
        if (_containersCli.IsSuccess(volumeName, true))
        {
            return 1;
        }

        if (!_containersCli.IsError($"Error response from daemon: get {volumeName}: no such volume", true))
        {
            throw new ContainersCliException("volume rm", 1, $"Unable to remove volume '{volumeName}'!");
        }

        LogMessage($"Volume '{volumeName}' not found!", "warn");
        return 0;
    }
    
    public bool NetworkExists(string networkName)
    {
        _containersCli.ClearArgsAndReset()
            .AddArg("network")
            .AddArg("ls")
            .AddFilterArg(networkName);
        LogCommand();
        _containersCli.Execute();
        LogExitCode();
        return _containersCli.IsSuccess(networkName);
    }

    public int CreateNetwork(NetworkConfig network, bool dryRun = false)
    {
        _containersCli.ClearArgsAndReset()
            .AddArg("network")
            .AddArg("create")
            .AddArg($"--driver {network.Driver}")
            .AddArgIf("--attachable", network.Attachable)
            .AddArgIf("--internal", network.Internal)
            .AddArgIf($"--ipam-driver {network.Ipam?.Driver}", !string.IsNullOrEmpty(network.Ipam?.Driver))
            .AddArgIf($"--subnet {network.Ipam?.Config.Subnet}", !string.IsNullOrEmpty(network.Ipam?.Config.Subnet))
            .AddArgIf($"--ip-range {network.Ipam?.Config.IpRange}", !string.IsNullOrEmpty(network.Ipam?.Config.IpRange))
            .AddArgIf($"--gateway {network.Ipam?.Config.Gateway}", !string.IsNullOrEmpty(network.Ipam?.Config.Gateway))
            .AddArg(network.Name);
        LogCommand();
        if (dryRun)
        {
            return 0;
        }
        _containersCli.Execute();
        LogExitCode();
        if (_containersCli is { ExitCode: 0, StdOutput.Count: 1 }
            && !string.IsNullOrEmpty(_containersCli.StdOutput.FirstOrDefault()))
        {
            return 1;
        }

        if (_containersCli.IsError($"network with name {network.Name} already exists", true))
        {
            return 0;
        }
        
        throw new ContainersCliException("network create", 1, $"Unable to create docker network '{network.Name}'!");
    }
    
    public int RemoveNetwork(string networkName, bool dryRun = false)
    {
        _containersCli.ClearArgsAndReset()
            .AddArg("network")
            .AddArg("rm")
            .AddArg(networkName);
        LogCommand();
        if (dryRun)
        {
            return 0;
        }
        _containersCli.Execute();
        LogExitCode();
        if (_containersCli.IsSuccess(networkName, true))
        {
            return 1;
        }

        if (!_containersCli.IsError($"Error response from daemon: network {networkName} not found", true))
        {
            throw new ContainersCliException("volume rm", 1, $"Unable to remove network '{networkName}'!");
        }

        LogMessage($"Network '{networkName}' not found!", "warn");
        return 0;
    }
    
    public bool PullImage(string image)
    {
        _containersCli.ClearArgsAndReset()
            .AddArg("pull")
            .AddArg(image);
        LogCommand();
        _containersCli.Execute();
        LogExitCode();
        if (_containersCli.IsSuccess($"Status: Downloaded newer image for {image}"))
        {
            return true;
        }
        if (_containersCli.IsSuccess($"Status: Image is up to date for {image}"))
        {
            return false;
        }
        
        throw new ContainersCliException("pull", _containersCli.ExitCode, $"Unable to pull image '{image}'!");
    }
    
    public string GetContainerImageId(string containerName)
    {
        _containersCli.ClearArgsAndReset()
            .AddArg("container")
            .AddArg("inspect")
            .AddArg("--format \"{{lower .Image}}\"")
            .AddArg(containerName);
        LogCommand();
        _containersCli.Execute();
        LogExitCode();
        if (_containersCli.IsSuccess() && _containersCli.StdOutput.Count == 1
            && _containersCli.StdOutput.First().StartsWith("sha256:"))
        {
            return _containersCli.StdOutput.First();
        }
    
        throw new ContainersCliException("container inspect", 1, $"Unable to inspect container '{containerName}'!");
    }

    public string GetImageId(string image)
    {
        _containersCli.ClearArgsAndReset()
            .AddArg("image")
            .AddArg("inspect")
            .AddArg("--format \"{{lower .Id}}\"")
            .AddArg(image);
        LogCommand();
        _containersCli.Execute();
        LogExitCode();
        if (_containersCli.IsSuccess() && _containersCli.StdOutput.Count == 1
                                   && _containersCli.StdOutput.First().StartsWith("sha256:"))
        {
            return _containersCli.StdOutput.First();
        }
        
        throw new ContainersCliException("image inspect", 1, $"Unable to inspect image '{image}'!");
    }

    public bool ArchiveDirectory(string targetDirectory, string archiveFile, bool dryRun = false)
    {
        var shellCommand = ShellCommand.GetShellCommandInstance(
                shell: ShellCommand.NoShell,
                command: ShellCommand.IsWindowsPlatform? "tar.exe" : "tar",
                onStdOutput: CommandStdOutputHandler,
                onErrOutput: CommandErrOutputHandler)
            .AddArg($"--directory={Directory.GetParent(targetDirectory)}")
            .AddArg("-pczf")
            .AddArg(archiveFile)
            .AddArg(Path.GetFileName(targetDirectory));
        LogCommand(shellCommand.ProcessFile, shellCommand.ProcessArguments);
        if (dryRun)
        {
            return false;
        }
        shellCommand.Execute();
        LogExitCode(shellCommand.ExitCode, shellCommand.ProcessFile, shellCommand.ProcessArguments);
        if (!shellCommand.IsSuccess())
        {
            throw new ShellCommandException(1, $"Unable to archive directory '{targetDirectory}' into '{archiveFile}'!");
        }

        return true;
    }
    
    public bool ArchiveVolume(string volumeName, string archiveFile, bool dryRun = false)
    {
        var tempVolumeSource = Directory.GetParent(archiveFile)?.ToString()
            ?? throw new ShellCommandException( 1, $"Invalid archive file path/name: {Directory.GetParent(archiveFile)}");
        
        _containersCli.ClearArgsAndReset()
            .AddArg("run")
            .AddArg("--rm")
            .AddVolumeArg(volumeName, "/source-volume", true)
            .AddVolumeArg(tempVolumeSource, "/backup")
            .AddArg("debian:12")
            .AddArg("tar")
            .AddArg("-C /source-volume")
            .AddArg("-pczf")
            .AddArg($"/backup/{Path.GetFileName(archiveFile)}")
            .AddArg(".");
        LogCommand();
        if (dryRun)
        {
            return false;
        }
        _containersCli.Execute();
        LogExitCode();
        if (_containersCli.IsSuccess())
        {
            return true;
        }

        throw new ContainersCliException("run", 1, $"Unable to archive volume '{volumeName}' into '{archiveFile}'!");
    }
    
    protected void LogMessage(string message, string level = "info")
    {
        if (OnContainersCliEvent is null)
        {
            return;
        }

        Dictionary<string, string?> eventData = new()
        {
            { "message", message },
            { "level", level }
        };
        OnContainersCliEvent.Invoke(this,new ContainersCliEventArgs(eventData, ContainersCliEventType.Message));
    }

    protected void LogCommand()
    {
        if (OnContainersCliEvent is null)
        {
            return;
        }

        _containersCli.Prepare();
        Dictionary<string, string?> eventData = new()
        {
            { "cmd", _containersCli.ProcessFile },
            { "args", _containersCli.ProcessArguments }
        };
        OnContainersCliEvent.Invoke(this,new ContainersCliEventArgs(eventData, ContainersCliEventType.Command));
    }
    
    protected void LogCommand(string cmd, string args)
    {
        if (OnContainersCliEvent is null)
        {
            return;
        }

        _containersCli.Prepare();
        Dictionary<string, string?> eventData = new()
        {
            { "cmd", cmd },
            { "args", args }
        };
        OnContainersCliEvent.Invoke(this,new ContainersCliEventArgs(eventData, ContainersCliEventType.Command));
    }

    protected void LogExitCode()
    {
        if (OnContainersCliEvent is null)
        {
            return;
        }

        _containersCli.Prepare();
        Dictionary<string, string?> eventData = new()
        {
            { "cmd", _containersCli.ProcessFile },
            { "args", _containersCli.ProcessArguments },
            { "exit", _containersCli.ExitCode.ToString() }
        };
        OnContainersCliEvent.Invoke(this,new ContainersCliEventArgs(eventData, ContainersCliEventType.ExitCode));
    }
    
    protected void LogExitCode(int exitCode, string cmd, string args)
    {
        if (OnContainersCliEvent is null)
        {
            return;
        }

        Dictionary<string, string?> eventData = new()
        {
            { "cmd", cmd },
            { "args", args },
            { "exit", exitCode.ToString() }
        };
        OnContainersCliEvent.Invoke(this,new ContainersCliEventArgs(eventData, ContainersCliEventType.ExitCode));
    }
    
    protected void CommandStdOutputHandler(object sender, OutputReceivedEventArgs e) => 
        OnContainersCliEvent?.Invoke(this,new ContainersCliEventArgs(e.Data, ContainersCliEventType.StdOutput));

    protected void CommandErrOutputHandler(object sender, OutputReceivedEventArgs e) => 
        OnContainersCliEvent?.Invoke(this,new ContainersCliEventArgs(e.Data, ContainersCliEventType.ErrOutput));
}