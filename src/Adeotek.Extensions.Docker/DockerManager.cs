using Adeotek.Extensions.Docker.Config;
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
    
    public bool CreateContainer(ContainerConfig config, bool dryRun = false)
    {
        _dockerCli.ClearArgsAndReset()
            .AddArg("run")
            .AddArg("-d")
            .AddArg($"--name={config.PrimaryName}")
            .AddPortsArgs(config.Ports)
            .AddVolumesArgs(config.Volumes)
            .AddEnvVarsArgs(config.EnvVars)
            .AddNetworkArgs(config)
            .AddRestartArg(config.Restart)
            .AddArg(config.FullImageName);
        
        LogCommand();
        if (dryRun)
        {
            return true;
        }
        _dockerCli.Execute();
        LogExitCode();
        if (_dockerCli is { ExitCode: 0, StdOutput.Count: 1 }
            && !string.IsNullOrEmpty(_dockerCli.StdOutput.FirstOrDefault()))
        {
            return true;
        }

        if (_dockerCli.IsError(config.PrimaryName))
        {
            return false;
        }
        
        throw new DockerCliException("run", 1, $"Unable to create container '{config.PrimaryName}'!");
    }
    
    public bool StartContainer(string containerName, bool dryRun = false)
    {
        _dockerCli.ClearArgsAndReset()
            .AddArg("start")
            .AddArg(containerName);
        LogCommand();
        if (dryRun)
        {
            return true;
        }
        _dockerCli.Execute();
        LogExitCode();
        if (_dockerCli.IsSuccess(containerName, true))
        {
            return true;
        }
        
        if (!_dockerCli.IsError($"Error response from daemon: No such container: {containerName}", true))
        {
            throw new DockerCliException("start", 1, $"Unable to stop container '{containerName}'!");    
        }
            
        LogMessage($"Container '{containerName}' not found!", "warn");
        return false;
    }
    
    public bool StopContainer(string containerName, bool dryRun = false)
    {
        _dockerCli.ClearArgsAndReset()
            .AddArg("stop")
            .AddArg(containerName);
        LogCommand();
        if (dryRun)
        {
            return false;
        }
        _dockerCli.Execute();
        LogExitCode();
        if (_dockerCli.IsSuccess(containerName, true))
        {
            return true;
        }
        
        if (!_dockerCli.IsError($"Error response from daemon: No such container: {containerName}", true))
        {
            throw new DockerCliException("stop", 1, $"Unable to stop container '{containerName}'!");    
        }
            
        LogMessage($"Container '{containerName}' not found!", "warn");
        return false;
    }
    
    public bool RemoveContainer(string containerName, bool dryRun = false)
    {
        _dockerCli.ClearArgsAndReset()
            .AddArg("rm")
            .AddArg(containerName);
        LogCommand();
        if (dryRun)
        {
            return false;
        }
        _dockerCli.Execute();
        LogExitCode();
        if (_dockerCli.IsSuccess(containerName, true))
        {
            return true;
        }

        if (!_dockerCli.IsError($"Error response from daemon: No such container: {containerName}", true))
        {
            throw new DockerCliException("rm", 1, $"Unable to remove container '{containerName}'!");
        }

        LogMessage($"Container '{containerName}' not found!", "warn");
        return false;
    }
    
    public bool RenameContainer(string currentName, string newName, bool dryRun = false)
    {
        _dockerCli.ClearArgsAndReset()
            .AddArg("rename")
            .AddArg(currentName)
            .AddArg(newName);
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

        if (!_dockerCli.IsError($"Error response from daemon: No such container: {currentName}", true))
        {
            throw new DockerCliException("rename", 1, $"Unable to rename container '{currentName}'!");
        }

        LogMessage($"Container '{currentName}' not found!", "warn");
        return false;
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

    public bool CreateVolume(string volumeName, bool dryRun = false)
    {
        _dockerCli.ClearArgsAndReset()
            .AddArg("volume")
            .AddArg("create")
            .AddArg(volumeName);
        LogCommand();
        if (dryRun)
        {
            return false;
        }
        
        _dockerCli.Execute();
        LogExitCode();
        if (!_dockerCli.IsSuccess(volumeName, true))
        {
            throw new DockerCliException("volume create", 1, $"Unable to create docker volume '{volumeName}'!");
        }

        return true;
    }

    public bool CreateMappedVolume(VolumeConfig volume, bool dryRun = false)
    {
        var madeChanges = false;
        LogCommand("mkdir", volume.Source);
        if (!dryRun)
        {
            Directory.CreateDirectory(volume.Source);
            madeChanges = true;
        }
        if (ShellCommand.IsWindowsPlatform)
        {
            return madeChanges;
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
            return false;
        }
        bashCommand.Execute();
        LogExitCode(bashCommand.ExitCode, bashCommand.ProcessFile, bashCommand.ProcessArguments);
        if (!bashCommand.IsSuccess(volume.Source))
        {
            throw new ShellCommandException(1, $"Unable to set group 'docker' for '{volume.Source}' directory!");
        }

        return true;
    }
    
    public bool RemoveVolume(string volumeName, bool dryRun = false)
    {
        _dockerCli.ClearArgsAndReset()
            .AddArg("volume")
            .AddArg("rm")
            .AddArg(volumeName);
        LogCommand();
        if (dryRun)
        {
            return false;
        }
        _dockerCli.Execute();
        LogExitCode();
        if (_dockerCli.IsSuccess(volumeName, true))
        {
            return true;
        }

        if (!_dockerCli.IsError($"Error response from daemon: get {volumeName}: no such volume", true))
        {
            throw new DockerCliException("volume rm", 1, $"Unable to remove volume '{volumeName}'!");
        }

        LogMessage($"Volume '{volumeName}' not found!", "warn");
        return false;
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

    public bool CreateNetwork(NetworkConfig network, bool dryRun = false)
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
            return false;
        }
        _dockerCli.Execute();
        LogExitCode();
        if (_dockerCli is { ExitCode: 0, StdOutput.Count: 1 }
            && !string.IsNullOrEmpty(_dockerCli.StdOutput.FirstOrDefault()))
        {
            return true;
        }

        if (_dockerCli.IsError($"network with name {network.Name} already exists", true))
        {
            return false;
        }
        
        throw new DockerCliException("network create", 1, $"Unable to create docker network '{network.Name}'!");
    }
    
    public bool RemoveNetwork(string networkName, bool dryRun = false)
    {
        _dockerCli.ClearArgsAndReset()
            .AddArg("network")
            .AddArg("rm")
            .AddArg(networkName);
        LogCommand();
        if (dryRun)
        {
            return false;
        }
        _dockerCli.Execute();
        LogExitCode();
        if (_dockerCli.IsSuccess(networkName, true))
        {
            return true;
        }

        if (!_dockerCli.IsError($"Error response from daemon: network {networkName} not found", true))
        {
            throw new DockerCliException("volume rm", 1, $"Unable to remove volume '{networkName}'!");
        }

        LogMessage($"Network '{networkName}' not found!", "warn");
        return false;
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
    #endregion

    #region Composed methods (untestable)
    public bool CheckAndCreateContainer(ContainerConfig config, bool dryRun = false)
    {
        var madeChanges = CreateNetworkIfMissing(config.Network, dryRun);

        madeChanges = config.Volumes
            .Aggregate(madeChanges, (current, volume) => CreateVolumeIfMissing(volume, dryRun) || current);

        return CreateContainer(config, dryRun) || madeChanges;
    }
    
    public bool UpgradeContainer(ContainerConfig config, bool replace = false, bool force = false, bool dryRun = false)
    {
        if (!CheckIfNewVersionExists(config))
        {
            if (!force)
            {
                LogMessage("No newer version found, nothing to do.", "msg");
                return false;    
            }
            LogMessage("No newer version found, forcing container recreation!", "warn");
        }

        var madeChanges = replace 
            ? StopAndRemoveContainer(config.PrimaryName, dryRun) 
            : DemoteContainer(config, dryRun);
    
        madeChanges = CheckAndCreateContainer(config, dryRun) || madeChanges;
        if (dryRun)
        {
            LogMessage("Container create finished.", "msg");
            LogMessage("Dry run: No changes were made!", "warn");
            return madeChanges;
        }
        
        LogMessage("Container updated successfully!", "msg");
        return madeChanges;
    }
    
    public bool StopAndRemoveContainer(string containerName, bool dryRun = false)
    {
        var madeChanges = StopContainer(containerName, dryRun);
        return RemoveContainer(containerName, dryRun) || madeChanges;
    }
    
    public bool StopAndRenameContainer(string currentName, string newName, bool dryRun = false)
    {
        var madeChanges = StopContainer(currentName, dryRun);
        return RenameContainer(currentName, newName, dryRun) || madeChanges;
    }
    
    public bool DemoteContainer(ContainerConfig config, bool dryRun = false)
    {
        var madeChanges = true;
        if (ContainerExists(config.BackupName))
        {
            madeChanges = StopAndRemoveContainer(config.BackupName, dryRun);
        }
    
        return StopAndRenameContainer(config.PrimaryName, config.BackupName, dryRun)
            || madeChanges;
    }

    public bool DowngradeContainer(ContainerConfig config, bool dryRun = false)
    {
        if (!ContainerExists(config.BackupName))
        {
            throw new DockerCliException("downgrade", 1, "Backup container is missing!");
        }

        var madeChanges = false;
        if (ContainerExists(config.PrimaryName))
        {
            madeChanges = StopAndRemoveContainer(config.PrimaryName, dryRun);    
        }
        
        madeChanges = RenameContainer(config.BackupName, config.PrimaryName, dryRun)
            || madeChanges;
        return StartContainer(config.PrimaryName, dryRun) || madeChanges;
    }
    
    public bool PurgeContainer(ContainerConfig config, bool purge = false, bool dryRun = false)
    {
        var madeChanges = StopAndRemoveContainer(config.PrimaryName, dryRun);
        if (!purge)
        {
            return madeChanges;
        }
    
        if (ContainerExists(config.BackupName))
        {
            LogMessage("Backup container found, removing it.");
            madeChanges = StopAndRemoveContainer(config.BackupName, dryRun) || madeChanges;
        }

        madeChanges = config.Volumes
            .Where(e => e is { AutoCreate: true, IsBind: false })
            .Aggregate(madeChanges, (current, volume) => RemoveVolume(volume.Source, dryRun) || current);

        if (config.Network is not null && !config.Network.IsShared)
        {
            madeChanges = RemoveNetwork(config.Network.Name, dryRun) || madeChanges;
        }

        return madeChanges;
    }

    public bool CreateVolumeIfMissing(VolumeConfig volume, bool dryRun = false)
    {
        if (volume.IsBind)
        {
            if (Path.Exists(volume.Source))
            {
                return false;
            }
    
            if (!volume.AutoCreate)
            {
                throw new DockerCliException("create container", 1, $"Docker volume '{volume.Source}' is missing or cannot be created!");
            }

            return CreateMappedVolume(volume, dryRun);
        }
        
        if (VolumeExists(volume.Source))
        {
            return false;
        }
    
        if (!volume.AutoCreate)
        {
            throw new DockerCliException("create container", 1, $"Docker volume '{volume.Source}' is missing or cannot be created!");
        }

        return CreateVolume(volume.Source, dryRun);
    }

    public bool CreateNetworkIfMissing(NetworkConfig? network, bool dryRun = false)
    {
        if (network is null || string.IsNullOrEmpty(network.Name))
        {
            return false;
        }

        return !NetworkExists(network.Name) && CreateNetwork(network, dryRun);
    }
    
    public bool CheckIfNewVersionExists(ContainerConfig config)
    {
        PullImage(config.Image, config.ImageTag);
        var imageId = GetImageId(config.Image, config.ImageTag);
        var containerImageId = GetContainerImageId(config.PrimaryName);
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
