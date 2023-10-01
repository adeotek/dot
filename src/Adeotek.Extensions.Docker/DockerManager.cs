﻿using Adeotek.Extensions.Docker.Config;
using Adeotek.Extensions.Docker.Exceptions;
using Adeotek.Extensions.Processes;

namespace Adeotek.Extensions.Docker;

public class DockerManager
{
    public delegate void DockerCliEventHandler(object sender, DockerCliEventArgs e);
    public event DockerCliEventHandler? OnDockerCliEvent;
    
    private readonly DockerCliCommand _dockerCli;

    public DockerManager()
    {
        _dockerCli = new DockerCliCommand();
        _dockerCli.OnStdOutput += CommandStdOutputHandler;
        _dockerCli.OnErrOutput += CommandErrOutputHandler;
    }

    public string[] LastStdOutput => _dockerCli.StdOutput.ToArray();
    public string[] LastErrOutput => _dockerCli.ErrOutput.ToArray();
    public int LastStatusCode => _dockerCli.StatusCode;

    public bool ContainerExists(string name)
    {
        _dockerCli.ClearArgsAndReset()
            .AddArg("container")
            .AddArg("ls")
            .AddArg("--all")
            .AddFilterArg(name);
        LogCommand();
        _dockerCli.Execute();
        LogExitCode();
        return _dockerCli.IsSuccess(name);
    }
    
    public bool CreateContainer(ContainerConfig config)
    {
        if (!CheckNetwork(config.Network))
        {
            throw new DockerCliException("create container", 1, $"Docker network '{config.Network?.Name}' is missing or cannot be created!");
        }
        
        foreach (var volume in config.Volumes)
        {
            if (!CheckVolume(volume))
            {
                throw new DockerCliException("create container", 1, $"Docker volume '{volume.Source}' is missing or cannot be created!");
            }
        }
    
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
        LogExitCode();
        _dockerCli.Execute();
        return _dockerCli.StatusCode == 0
               || _dockerCli.StdOutput.Count == 1
               || string.IsNullOrEmpty(_dockerCli.StdOutput.FirstOrDefault());
    }
    
    public void UpdateContainer(ContainerConfig config, bool replace = false)
    {
        if (!CheckIfNewVersionExists(config))
        {
            LogMessage("No newer version found, nothing to do.");
            return;
        }
        
        if (replace)
        {
            StopAndRemoveContainer(config.PrimaryName);
        }
        else
        {
            DemoteContainer(config);    
        }
    
        CreateContainer(config);
        LogMessage("Container updated successfully!", "msg");
    }
    
    public void StopContainer(string containerName)
    {
        _dockerCli.ClearArgsAndReset()
            .AddArg("stop")
            .AddArg(containerName);
        LogCommand();
        _dockerCli.Execute();
        LogExitCode();
        if (_dockerCli.IsSuccess(containerName, true))
        {
            return;
        }
        
        if (!_dockerCli.IsError($"Error response from daemon: No such container: {containerName}", true))
        {
            throw new DockerCliException("stop", 1, $"Unable to stop container '{containerName}'!");    
        }
            
        LogMessage($"Container '{containerName}' not found!", "warn");
    }
    
    public void StopAndRemoveContainer(string containerName)
    {
        StopContainer(containerName);
    
        _dockerCli.ClearArgsAndReset()
            .AddArg("rm")
            .AddArg(containerName);
        LogCommand();
        _dockerCli.Execute();
        LogExitCode();
        if (_dockerCli.IsSuccess(containerName, true))
        {
            return;
        }
    
        if (_dockerCli.IsError($"Error response from daemon: No such container: {containerName}", true))
        {
            LogMessage($"Container '{containerName}' not found!", "warn");
            return;
        }
        
        throw new DockerCliException("rm", 1, $"Unable to remove container '{containerName}'!");
    }
    
    public void RenameContainer(string currentName, string newName)
    {
        StopContainer(currentName);
    
        _dockerCli.ClearArgsAndReset()
            .AddArg("rename")
            .AddArg(currentName)
            .AddArg(newName);
        LogCommand();
        _dockerCli.Execute();
        LogExitCode();
        if (_dockerCli.IsSuccess())
        {
            return;
        }
    
        if (_dockerCli.IsError($"Error response from daemon: No such container: {currentName}", true))
        {
            LogMessage($"Container '{currentName}' not found!", "warn");
            return;
        }
        
        throw new DockerCliException("rename", 1, $"Unable to rename container '{currentName}'!");
    }
    
    public void DemoteContainer(ContainerConfig config)
    {
        if (ContainerExists(config.BackupName))
        {
            StopAndRemoveContainer(config.BackupName);
        }
    
        RenameContainer(config.PrimaryName, config.BackupName);
    }
    
    public void RemoveContainer(ContainerConfig config, bool purge = false)
    {
        StopAndRemoveContainer(config.PrimaryName);
        if (!purge)
        {
            return;
        }
    
        if (ContainerExists(config.BackupName))
        {
            LogMessage("Backup container found, removing it.");
            StopAndRemoveContainer(config.BackupName);    
        }
    
        foreach (var volume in config.Volumes.Where(e => e is { AutoCreate: true, IsMapping: false }))
        {
            _dockerCli.ClearArgsAndReset()
                .AddArg("volume")
                .AddArg("rm")
                .AddArg(volume.Source);
            LogCommand();
            _dockerCli.Execute();
            LogExitCode();
            if (_dockerCli.IsSuccess(volume.Source, true))
            {
                continue;
            }
            
            if (_dockerCli.IsError($"Error response from daemon: get {volume.Source}: no such volume", true))
            {
                LogMessage($"Volume '{volume.Source}' not found!", "warn");
                return;
            }
            
            throw new DockerCliException("volume rm", 1, $"Unable to remove volume '{volume.Source}'!");
        }
    }
    
    public string PullImage(string image, string? tag = null)
    {
        var fullImageName = $"{image}:{tag ?? "latest"}";
        _dockerCli.ClearArgsAndReset()
            .AddArg("pull")
            .AddArg(fullImageName);
        LogCommand();
        _dockerCli.Execute();
        LogExitCode();
        if (_dockerCli.StatusCode != 0
            || !_dockerCli.StdOutput.Exists(e => 
                e.Contains($"Status: Downloaded newer image for {fullImageName}")
                || e.Contains($"Status: Image is up to date for {fullImageName}")))
        {
            throw new DockerCliException("pull", _dockerCli.StatusCode, $"Unable to pull image '{fullImageName}'!");
        }
        
        return _dockerCli.StdOutput
                   .FirstOrDefault(e => e.StartsWith("Digest: "))
                   ?.Replace("Digest: ", "")
               ?? "";
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
        if (_dockerCli.IsError() || _dockerCli.StdOutput.Count != 1)
        {
            throw new DockerCliException("container inspect", 1, $"Unable to inspect container '{containerName}'!");
        }
    
        var result = _dockerCli.StdOutput.First();
        if (result.StartsWith("sha256:"))
        {
            return result;
        }
    
        throw new DockerCliException("container inspect", 1, $"Unable to obtain image ID for container '{containerName}'!");
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
        if (_dockerCli.IsError() || _dockerCli.StdOutput.Count != 1)
        {
            throw new DockerCliException("image inspect", 1, $"Unable to inspect image '{fullImageName}'!");
        }

        var result = _dockerCli.StdOutput.First();
        if (result.StartsWith("sha256:"))
        {
            return result;
        }
        
        throw new DockerCliException("image inspect", 1, $"Unable to obtain image ID for '{fullImageName}'!");
    }
    
    public bool CheckIfNewVersionExists(ContainerConfig config)
    {
        var imageId = PullImage(config.Image, config.ImageTag);
        var containerImageId = GetContainerImageId(config.PrimaryName);
        return containerImageId.Equals(imageId, StringComparison.InvariantCultureIgnoreCase);
    }
    
    public bool CheckVolume(VolumeConfig volume)
    {
        if (volume.IsMapping)
        {
            if (Path.Exists(volume.Source))
            {
                return true;
            }
    
            if (!volume.AutoCreate)
            {
                return false;
            }
    
            LogCommand("mkdir", volume.Source);
            Directory.CreateDirectory(volume.Source);
            if (ShellCommand.IsWindowsPlatform)
            {
                return true;
            }

            var bashCommand = new ShellCommand(ShellCommand.BashShell) { Command = "chgrp" };
            bashCommand.OnStdOutput += CommandStdOutputHandler;
            bashCommand.OnErrOutput += CommandErrOutputHandler;
            bashCommand
                .AddArg("docker")
                .AddArg(volume.Source);
            LogCommand(bashCommand.ProcessFile, bashCommand.ProcessArguments);
            bashCommand.Execute();
            LogExitCode(bashCommand.StatusCode, bashCommand.ProcessFile, bashCommand.ProcessArguments);
            if (!bashCommand.IsSuccess(volume.Source))
            {
                throw new ShellCommandException(1, $"Unable to set group 'docker' for '{volume.Source}' directory!");
            }

            return true;
        }
        
        _dockerCli.ClearArgsAndReset()
            .AddArg("volume")
            .AddArg("ls")
            .AddFilterArg(volume.Source);
        LogCommand();
        _dockerCli.Execute();
        LogExitCode();
        if (_dockerCli.IsSuccess(volume.Source, true))
        {
            return true;
        }
    
        if (!volume.AutoCreate)
        {
            return false;
        }
    
        _dockerCli.ClearArgsAndReset()
            .AddArg("volume")
            .AddArg("create")
            .AddArg(volume.Source);
        LogCommand();
        _dockerCli.Execute();
        LogExitCode();
        if (!_dockerCli.IsSuccess(volume.Source, true))
        {
            throw new DockerCliException("volume create", 1, $"Unable to create docker volume '{volume.Source}'!");
        }
    
        return true;
    }
    
    public bool CheckNetwork(NetworkConfig? network)
    {
        if (network is null || string.IsNullOrEmpty(network.Name))
        {
            return true;
        }
        
        if (string.IsNullOrEmpty(network.Subnet) || string.IsNullOrEmpty(network.IpRange))
        {
            return false;
        }
        
        _dockerCli.ClearArgsAndReset()
            .AddArg("network")
            .AddArg("ls")
            .AddFilterArg(network.Name);
        LogCommand();
        _dockerCli.Execute();
        LogExitCode();
        if (_dockerCli.IsSuccess(network.Name))
        {
            return true;
        }
    
        _dockerCli.ClearArgsAndReset()
            .AddArg("network")
            .AddArg("create")
            .AddArg("-d bridge")
            .AddArg("--attachable")
            .AddArg($"--subnet {network.Subnet}")
            .AddArg($"--ip-range {network.IpRange}")
            .AddArg(network.Name);
        LogCommand();
        _dockerCli.Execute();
        LogExitCode();
        if (_dockerCli.StatusCode != 0 
            || _dockerCli.StdOutput.Count != 1
            || string.IsNullOrEmpty(_dockerCli.StdOutput.FirstOrDefault()))
        {
            throw new DockerCliException("network create", 1, $"Unable to create docker network '{network.Name}'!");
        }
    
        return true;
    }
    
    private void LogMessage(string message, string level = "info")
    {
        if (OnDockerCliEvent is null)
        {
            return;
        }

        _dockerCli.Prepare();
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
            { "exit", _dockerCli.StatusCode.ToString() }
        };
        OnDockerCliEvent.Invoke(this,new DockerCliEventArgs(eventData, DockerCliEventType.ExitCode));
    }
    
    private void LogExitCode(int exitCode, string cmd, string args)
    {
        if (OnDockerCliEvent is null)
        {
            return;
        }

        _dockerCli.Prepare();
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
}