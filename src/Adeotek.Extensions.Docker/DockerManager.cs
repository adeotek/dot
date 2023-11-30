using Adeotek.Extensions.Docker.Config;
using Adeotek.Extensions.Docker.Config.V1;
using Adeotek.Extensions.Docker.Exceptions;
using Adeotek.Extensions.Processes;

namespace Adeotek.Extensions.Docker;

public class DockerManager
{
    public delegate void DockerCliEventHandler(object sender, DockerCliEventArgs e);
    public event DockerCliEventHandler? OnDockerCliEvent;
    
    private readonly DockerCliCommand _dockerCli;

    public DockerManager(DockerCliCommand? dockerCli = null)
    {
        _dockerCli = dockerCli ?? DockerCliCommand.GetDockerCliCommandInstance(
            CommandStdOutputHandler, CommandErrOutputHandler);
    }

    public string[] LastStdOutput => _dockerCli.StdOutput.ToArray();
    public string[] LastErrOutput => _dockerCli.ErrOutput.ToArray();
    public int LastStatusCode => _dockerCli.ExitCode;

    #region Primary methods (testable)
    public bool ContainerExists(string name)
    {
        _dockerCli.ClearArgsAndReset()
            .AddArg("container")
            .AddArg("inspect")
            .AddArg("--format \"{{lower .Name}}\"")
            .AddArg(name);
        LogCommand();
        _dockerCli.Execute();
        LogExitCode();
        if (_dockerCli.IsSuccess(name))
        {
            return true;
        }

        if (_dockerCli.IsError(name))
        {
            return false;
        }
        
        throw new DockerCliException("container inspect", 1, $"Unable to inspect container '{name}'!");
    }
    
    public int CreateContainer(ContainerConfigV1 configV1, bool dryRun = false)
    {
        _dockerCli.ClearArgsAndReset()
            .AddArg("run")
            .AddRunCommandOptionsArgs(configV1.RunCommandOptions)
            .AddArg($"--name={configV1.CurrentName}")
            .AddPortsArgs(configV1.Ports)
            .AddVolumesArgs(configV1.Volumes)
            .AddEnvVarsArgs(configV1.EnvVars)
            .AddNetworkArgs(configV1)
            .AddExtraHostsArgs(configV1.ExtraHosts)
            .AddRestartArg(configV1.Restart)
            .AddArg(configV1.FullImageName)
            .AddStartupCommandArgs(configV1);
        
        LogCommand();
        if (dryRun)
        {
            return 0;
        }
        _dockerCli.Execute();
        LogExitCode();
        if (_dockerCli is { ExitCode: 0, StdOutput.Count: 1 }
            && !string.IsNullOrEmpty(_dockerCli.StdOutput.FirstOrDefault()))
        {
            return 1;
        }

        if (_dockerCli.IsError(configV1.CurrentName))
        {
            return 0;
        }
        
        throw new DockerCliException("run", 1, $"Unable to create container '{configV1.CurrentName}'!");
    }
    
    public int StartContainer(string containerName, bool dryRun = false)
    {
        _dockerCli.ClearArgsAndReset()
            .AddArg("start")
            .AddArg(containerName);
        LogCommand();
        if (dryRun)
        {
            return 0;
        }
        _dockerCli.Execute();
        LogExitCode();
        if (_dockerCli.IsSuccess(containerName, true))
        {
            return 1;
        }
        
        if (!_dockerCli.IsError($"Error response from daemon: No such container: {containerName}", true))
        {
            throw new DockerCliException("start", 1, $"Unable to stop container '{containerName}'!");    
        }
            
        LogMessage($"Container '{containerName}' not found!", "warn");
        return 0;
    }
    
    public int StopContainer(string containerName, bool dryRun = false)
    {
        _dockerCli.ClearArgsAndReset()
            .AddArg("stop")
            .AddArg(containerName);
        LogCommand();
        if (dryRun)
        {
            return 0;
        }
        _dockerCli.Execute();
        LogExitCode();
        if (_dockerCli.IsSuccess(containerName, true))
        {
            return 1;
        }
        
        if (!_dockerCli.IsError($"Error response from daemon: No such container: {containerName}", true))
        {
            throw new DockerCliException("stop", 1, $"Unable to stop container '{containerName}'!");    
        }
            
        LogMessage($"Container '{containerName}' not found!", "warn");
        return 0;
    }
    
    public int RemoveContainer(string containerName, bool dryRun = false)
    {
        _dockerCli.ClearArgsAndReset()
            .AddArg("rm")
            .AddArg(containerName);
        LogCommand();
        if (dryRun)
        {
            return 0;
        }
        _dockerCli.Execute();
        LogExitCode();
        if (_dockerCli.IsSuccess(containerName, true))
        {
            return 1;
        }

        if (!_dockerCli.IsError($"Error response from daemon: No such container: {containerName}", true))
        {
            throw new DockerCliException("rm", 1, $"Unable to remove container '{containerName}'!");
        }

        LogMessage($"Container '{containerName}' not found!", "warn");
        return 0;
    }
    
    public int RenameContainer(string currentName, string newName, bool dryRun = false)
    {
        _dockerCli.ClearArgsAndReset()
            .AddArg("rename")
            .AddArg(currentName)
            .AddArg(newName);
        LogCommand();
        if (dryRun)
        {
            return 0;
        }
        _dockerCli.Execute();
        LogExitCode();
        if (_dockerCli.IsSuccess())
        {
            return 1;
        }

        if (!_dockerCli.IsError($"Error response from daemon: No such container: {currentName}", true))
        {
            throw new DockerCliException("rename", 1, $"Unable to rename container '{currentName}'!");
        }

        LogMessage($"Container '{currentName}' not found!", "warn");
        return 0;
    }

    public bool VolumeExists(string volumeName)
    {
        _dockerCli.ClearArgsAndReset()
            .AddArg("volume")
            .AddArg("ls")
            .AddFilterArg(volumeName);
        LogCommand();
        _dockerCli.Execute();
        LogExitCode();
        return _dockerCli.IsSuccess(volumeName);
    }

    public int CreateVolume(string volumeName, bool dryRun = false)
    {
        _dockerCli.ClearArgsAndReset()
            .AddArg("volume")
            .AddArg("create")
            .AddArg(volumeName);
        LogCommand();
        if (dryRun)
        {
            return 0;
        }
        
        _dockerCli.Execute();
        LogExitCode();
        if (!_dockerCli.IsSuccess(volumeName, true))
        {
            throw new DockerCliException("volume create", 1, $"Unable to create docker volume '{volumeName}'!");
        }

        return 1;
    }

    public int CreateBindVolume(VolumeConfigV1 volume, bool dryRun = false)
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
        _dockerCli.ClearArgsAndReset()
            .AddArg("volume")
            .AddArg("rm")
            .AddArg(volumeName);
        LogCommand();
        if (dryRun)
        {
            return 0;
        }
        _dockerCli.Execute();
        LogExitCode();
        if (_dockerCli.IsSuccess(volumeName, true))
        {
            return 1;
        }

        if (!_dockerCli.IsError($"Error response from daemon: get {volumeName}: no such volume", true))
        {
            throw new DockerCliException("volume rm", 1, $"Unable to remove volume '{volumeName}'!");
        }

        LogMessage($"Volume '{volumeName}' not found!", "warn");
        return 0;
    }
    
    public bool NetworkExists(string networkName)
    {
        _dockerCli.ClearArgsAndReset()
            .AddArg("network")
            .AddArg("ls")
            .AddFilterArg(networkName);
        LogCommand();
        _dockerCli.Execute();
        LogExitCode();
        return _dockerCli.IsSuccess(networkName);
    }

    public int CreateNetwork(NetworkConfigV1 network, bool dryRun = false)
    {
        _dockerCli.ClearArgsAndReset()
            .AddArg("network")
            .AddArg("create")
            .AddArg("-d bridge")
            .AddArg("--attachable")
            .AddArg($"--subnet {network.Subnet}")
            .AddArg($"--ip-range {network.IpRange}")
            .AddArg(network.Name);
        LogCommand();
        if (dryRun)
        {
            return 0;
        }
        _dockerCli.Execute();
        LogExitCode();
        if (_dockerCli is { ExitCode: 0, StdOutput.Count: 1 }
            && !string.IsNullOrEmpty(_dockerCli.StdOutput.FirstOrDefault()))
        {
            return 1;
        }

        if (_dockerCli.IsError($"network with name {network.Name} already exists", true))
        {
            return 0;
        }
        
        throw new DockerCliException("network create", 1, $"Unable to create docker network '{network.Name}'!");
    }
    
    public int RemoveNetwork(string networkName, bool dryRun = false)
    {
        _dockerCli.ClearArgsAndReset()
            .AddArg("network")
            .AddArg("rm")
            .AddArg(networkName);
        LogCommand();
        if (dryRun)
        {
            return 0;
        }
        _dockerCli.Execute();
        LogExitCode();
        if (_dockerCli.IsSuccess(networkName, true))
        {
            return 1;
        }

        if (!_dockerCli.IsError($"Error response from daemon: network {networkName} not found", true))
        {
            throw new DockerCliException("volume rm", 1, $"Unable to remove network '{networkName}'!");
        }

        LogMessage($"Network '{networkName}' not found!", "warn");
        return 0;
    }
    
    public bool PullImage(string image, string? tag = null)
    {
        var fullImageName = $"{image}:{tag ?? "latest"}";
        _dockerCli.ClearArgsAndReset()
            .AddArg("pull")
            .AddArg(fullImageName);
        LogCommand();
        _dockerCli.Execute();
        LogExitCode();
        if (_dockerCli.IsSuccess($"Status: Downloaded newer image for {fullImageName}"))
        {
            return true;
        }
        if (_dockerCli.IsSuccess($"Status: Image is up to date for {fullImageName}"))
        {
            return false;
        }
        
        throw new DockerCliException("pull", _dockerCli.ExitCode, $"Unable to pull image '{fullImageName}'!");
    }
    
    public string GetContainerImageId(string containerName)
    {
        _dockerCli.ClearArgsAndReset()
            .AddArg("container")
            .AddArg("inspect")
            .AddArg("--format \"{{lower .Image}}\"")
            .AddArg(containerName);
        LogCommand();
        _dockerCli.Execute();
        LogExitCode();
        if (_dockerCli.IsSuccess() && _dockerCli.StdOutput.Count == 1
            && _dockerCli.StdOutput.First().StartsWith("sha256:"))
        {
            return _dockerCli.StdOutput.First();
        }
    
        throw new DockerCliException("container inspect", 1, $"Unable to inspect container '{containerName}'!");
    }

    public string GetImageId(string image, string? tag = null)
    {
        var fullImageName = $"{image}:{tag ?? "latest"}";
        _dockerCli.ClearArgsAndReset()
            .AddArg("image")
            .AddArg("inspect")
            .AddArg("--format \"{{lower .Id}}\"")
            .AddArg(fullImageName);
        LogCommand();
        _dockerCli.Execute();
        LogExitCode();
        if (_dockerCli.IsSuccess() && _dockerCli.StdOutput.Count == 1
                                   && _dockerCli.StdOutput.First().StartsWith("sha256:"))
        {
            return _dockerCli.StdOutput.First();
        }
        
        throw new DockerCliException("image inspect", 1, $"Unable to inspect image '{fullImageName}'!");
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
        
        _dockerCli.ClearArgsAndReset()
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
        _dockerCli.Execute();
        LogExitCode();
        if (_dockerCli.IsSuccess())
        {
            return true;
        }

        throw new DockerCliException("run", 1, $"Unable to archive volume '{volumeName}' into '{archiveFile}'!");
    }
    #endregion

    #region Composed methods (untestable)
    public int CheckAndCreateContainer(ContainerConfigV1 configV1, bool dryRun = false) =>
        CreateNetworkIfMissing(configV1.Network, dryRun)
        + configV1.Volumes.Sum(volume => CreateVolumeIfMissing(volume, dryRun))
        + CreateContainer(configV1, dryRun);

    public int UpgradeContainer(ContainerConfigV1 configV1, bool replace = false, bool force = false, bool dryRun = false)
    {
        if (!CheckIfNewVersionExists(configV1))
        {
            if (!force)
            {
                LogMessage("No newer version found, nothing to do.", "msg");
                return 0;    
            }
            LogMessage("No newer version found, forcing container recreation!", "warn");
        }

        var changes = replace 
            ? StopAndRemoveContainer(configV1.CurrentName, dryRun) 
            : DemoteContainer(configV1, dryRun);
    
        changes += CheckAndCreateContainer(configV1, dryRun);
        if (dryRun)
        {
            LogMessage("Container create finished.", "msg");
            LogMessage("Dry run: No changes were made!", "warn");
            return changes;
        }
        
        LogMessage("Container updated successfully!", "msg");
        return changes;
    }
    
    public int StopAndRemoveContainer(string containerName, bool dryRun = false) =>
        StopContainer(containerName, dryRun)
        + RemoveContainer(containerName, dryRun);

    public int StopAndRenameContainer(string currentName, string newName, bool dryRun = false) =>
        StopContainer(currentName, dryRun)
        + RenameContainer(currentName, newName, dryRun);

    public int DemoteContainer(ContainerConfigV1 configV1, bool dryRun = false) =>
        (ContainerExists(configV1.PreviousName) 
            ? StopAndRemoveContainer(configV1.PreviousName, dryRun) 
            : 0)
        + StopAndRenameContainer(configV1.CurrentName, configV1.PreviousName, dryRun);

    public int DowngradeContainer(ContainerConfigV1 configV1, bool dryRun = false)
    {
        if (!ContainerExists(configV1.PreviousName))
        {
            throw new DockerCliException("downgrade", 1, "Previous container is missing!");
        }

        return (ContainerExists(configV1.CurrentName)
               ? StopAndRemoveContainer(configV1.CurrentName, dryRun)
               : 0)
           + RenameContainer(configV1.PreviousName, configV1.CurrentName, dryRun)
           + StartContainer(configV1.CurrentName, dryRun);
    }
    
    public int PurgeContainer(ContainerConfigV1 configV1, bool purge = false, bool dryRun = false)
    {
        var changes = StopAndRemoveContainer(configV1.CurrentName, dryRun);
        if (!purge)
        {
            return changes;
        }
    
        if (ContainerExists(configV1.PreviousName))
        {
            LogMessage("Previous container found, removing it.");
            changes += StopAndRemoveContainer(configV1.PreviousName, dryRun);
        }

        changes += configV1.Volumes
            .Where(e => e is { AutoCreate: true, IsBind: false })
            .Sum(volume => RemoveVolume(volume.Source, dryRun));

        if (configV1.Network is not null && !configV1.Network.IsShared)
        {
            changes += RemoveNetwork(configV1.Network.Name, dryRun);
        }

        return changes;
    }

    public int CreateVolumeIfMissing(VolumeConfigV1 volume, bool dryRun = false)
    {
        if (volume.IsBind)
        {
            if (Path.Exists(volume.Source))
            {
                return 0;
            }
    
            if (!volume.AutoCreate)
            {
                throw new DockerCliException("create container", 1, $"Docker volume '{volume.Source}' is missing or cannot be created!");
            }

            return CreateBindVolume(volume, dryRun);
        }
        
        if (VolumeExists(volume.Source))
        {
            return 0;
        }
    
        if (!volume.AutoCreate)
        {
            throw new DockerCliException("create container", 1, $"Docker volume '{volume.Source}' is missing or cannot be created!");
        }

        return CreateVolume(volume.Source, dryRun);
    }

    public int CreateNetworkIfMissing(NetworkConfigV1? network, bool dryRun = false)
    {
        if (network is null || string.IsNullOrEmpty(network.Name) || NetworkExists(network.Name))
        {
            return 0;
        }

        return CreateNetwork(network, dryRun);
    }
    
    public bool CheckIfNewVersionExists(ContainerConfigV1 configV1)
    {
        PullImage(configV1.Image, configV1.ImageTag);
        var imageId = GetImageId(configV1.Image, configV1.ImageTag);
        var containerImageId = GetContainerImageId(configV1.CurrentName);
        return !string.IsNullOrEmpty(imageId) 
            && !imageId.Equals(containerImageId, StringComparison.InvariantCultureIgnoreCase);
    }
    #endregion

    #region Helper methods (private)
    private void LogMessage(string message, string level = "info")
    {
        if (OnDockerCliEvent is null)
        {
            return;
        }

        Dictionary<string, string?> eventData = new()
        {
            { "message", message },
            { "level", level }
        };
        OnDockerCliEvent.Invoke(this,new DockerCliEventArgs(eventData, DockerCliEventType.Message));
    }

    private void LogCommand()
    {
        if (OnDockerCliEvent is null)
        {
            return;
        }

        _dockerCli.Prepare();
        Dictionary<string, string?> eventData = new()
        {
            { "cmd", _dockerCli.ProcessFile },
            { "args", _dockerCli.ProcessArguments }
        };
        OnDockerCliEvent.Invoke(this,new DockerCliEventArgs(eventData, DockerCliEventType.Command));
    }
    
    private void LogCommand(string cmd, string args)
    {
        if (OnDockerCliEvent is null)
        {
            return;
        }

        _dockerCli.Prepare();
        Dictionary<string, string?> eventData = new()
        {
            { "cmd", cmd },
            { "args", args }
        };
        OnDockerCliEvent.Invoke(this,new DockerCliEventArgs(eventData, DockerCliEventType.Command));
    }

    private void LogExitCode()
    {
        if (OnDockerCliEvent is null)
        {
            return;
        }

        _dockerCli.Prepare();
        Dictionary<string, string?> eventData = new()
        {
            { "cmd", _dockerCli.ProcessFile },
            { "args", _dockerCli.ProcessArguments },
            { "exit", _dockerCli.ExitCode.ToString() }
        };
        OnDockerCliEvent.Invoke(this,new DockerCliEventArgs(eventData, DockerCliEventType.ExitCode));
    }
    
    private void LogExitCode(int exitCode, string cmd, string args)
    {
        if (OnDockerCliEvent is null)
        {
            return;
        }

        Dictionary<string, string?> eventData = new()
        {
            { "cmd", cmd },
            { "args", args },
            { "exit", exitCode.ToString() }
        };
        OnDockerCliEvent.Invoke(this,new DockerCliEventArgs(eventData, DockerCliEventType.ExitCode));
    }
    
    private void CommandStdOutputHandler(object sender, OutputReceivedEventArgs e) => 
        OnDockerCliEvent?.Invoke(this,new DockerCliEventArgs(e.Data, DockerCliEventType.StdOutput));

    private void CommandErrOutputHandler(object sender, OutputReceivedEventArgs e) => 
        OnDockerCliEvent?.Invoke(this,new DockerCliEventArgs(e.Data, DockerCliEventType.ErrOutput));
    #endregion
}
