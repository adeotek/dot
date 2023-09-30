﻿// using Adeotek.Extensions.Docker.Config;
// using Adeotek.Extensions.Processes;
//
// namespace Adeotek.Extensions.Docker;
//
// public class DockerManager
// {
//     protected virtual bool CreateContainer(ContainerConfig config)
//     {
//         if (!CheckNetwork(config.Network))
//         {
//             throw new ShellCommandException(1, $"Docker network '{config.Network?.Name}' is missing or cannot be created!");
//         }
//         
//         foreach (var volume in config.Volumes)
//         {
//             if (!CheckVolume(volume))
//             {
//                 throw new ShellCommandException(1, $"Docker volume '{volume.Source}' is missing or cannot be created!");
//             }
//         }
//
//         var dockerCommand = GetDockerCliCommand(IsVerbose)
//             .AddArg("run")
//             .AddArg("-d")
//             .AddArg($"--name={config.PrimaryName}")
//             .AddPortsArgs(config.Ports)
//             .AddVolumesArgs(config.Volumes)
//             .AddEnvVarsArgs(config.EnvVars)
//             .AddNetworkArgs(config)
//             .AddRestartArg(config.Restart)
//             .AddArg(config.FullImageName);
//         
//         PrintCommand(dockerCommand);
//         _errOutputColor = null;
//         dockerCommand.Execute();
//         _errOutputColor = null;
//         return dockerCommand.StatusCode == 0
//             || dockerCommand.StdOutput.Count == 1
//             || string.IsNullOrEmpty(dockerCommand.StdOutput.FirstOrDefault());
//     }
//
//     protected virtual void UpdateContainer(ContainerConfig config, bool replace = false)
//     {
//         if (!CheckIfNewVersionExists(config))
//         {
//             PrintMessage("No newer version found, nothing to do.");
//             return;
//         }
//         
//         if (replace)
//         {
//             StopAndRemoveContainer(config.PrimaryName);
//         }
//         else
//         {
//             DemoteContainer(config);    
//         }
//
//         CreateContainer(config);
//         PrintMessage("Container updated successfully!", _successColor);
//     }
//
//     protected virtual void DemoteContainer(ContainerConfig config)
//     {
//         if (CheckIfContainerExists(config.BackupName))
//         {
//             StopAndRemoveContainer(config.BackupName);
//         }
//
//         RenameContainer(config.PrimaryName, config.BackupName);
//     }
//
//     protected virtual void RemoveContainer(ContainerConfig config, bool purge = false)
//     {
//         StopAndRemoveContainer(config.PrimaryName);
//         if (!purge)
//         {
//             return;
//         }
//
//         if (CheckIfContainerExists(config.BackupName))
//         {
//             PrintMessage("Backup container found, removing it.", _standardColor);
//             StopAndRemoveContainer(config.BackupName);    
//         }
//
//         var dockerCommand = GetDockerCliCommand(IsVerbose);
//         foreach (var volume in config.Volumes.Where(e => e is { AutoCreate: true, IsMapping: false }))
//         {
//             dockerCommand.ClearArgs()
//                 .AddArg("volume")
//                 .AddArg("rm")
//                 .AddArg(volume.Source);
//             PrintCommand(dockerCommand);
//             dockerCommand.Execute();
//             if (dockerCommand.IsSuccess(volume.Source, true))
//             {
//                 continue;
//             }
//             
//             if (dockerCommand.IsError($"Error response from daemon: get {volume.Source}: no such volume", true))
//             {
//                 PrintMessage($"Volume '{volume.Source}' not found!", _warningColor);
//                 return;
//             }
//             
//             throw new ShellCommandException(1, $"Unable to remove volume '{volume.Source}'!");
//         }
//     }
//     
//     protected virtual void StopAndRemoveContainer(string containerName)
//     {
//         StopContainer(containerName);
//
//         var dockerCommand = GetDockerCliCommand(IsVerbose)
//             .AddArg("rm")
//             .AddArg(containerName);
//         PrintCommand(dockerCommand);
//         dockerCommand.Execute();
//         if (dockerCommand.IsSuccess(containerName, true))
//         {
//             return;
//         }
//
//         if (dockerCommand.IsError($"Error response from daemon: No such container: {containerName}", true))
//         {
//             PrintMessage($"Container '{containerName}' not found!", _warningColor);
//             return;
//         }
//         
//         throw new ShellCommandException(1, $"Unable to remove container '{containerName}'!");
//     }
//
//     protected virtual void StopContainer(string containerName)
//     {
//         var dockerCommand = GetDockerCliCommand(IsVerbose)
//             .AddArg("stop")
//             .AddArg(containerName);
//         PrintCommand(dockerCommand);
//         dockerCommand.Execute();
//         if (dockerCommand.IsSuccess(containerName, true))
//         {
//             return;
//         }
//         
//         if (!dockerCommand.IsError($"Error response from daemon: No such container: {containerName}", true))
//         {
//             throw new ShellCommandException(1, $"Unable to stop container '{containerName}'!");    
//         }
//             
//         PrintMessage($"Container '{containerName}' not found!", _warningColor);
//     }
//     
//     protected virtual void RenameContainer(string currentName, string newName)
//     {
//         StopContainer(currentName);
//
//         var dockerCommand = GetDockerCliCommand(IsVerbose)
//             .AddArg("rename")
//             .AddArg(currentName)
//             .AddArg(newName);
//         PrintCommand(dockerCommand);
//         dockerCommand.Execute();
//         if (dockerCommand.IsSuccess())
//         {
//             return;
//         }
//
//         if (dockerCommand.IsError($"Error response from daemon: No such container: {currentName}", true))
//         {
//             PrintMessage($"Container '{currentName}' not found!", _warningColor);
//             return;
//         }
//         
//         throw new ShellCommandException(1, $"Unable to rename container '{currentName}'!");
//     }
//     
//     protected virtual string PullImage(string image, string? tag = null)
//     {
//         var fullImageName = $"{image}:{tag ?? "latest"}";
//         var dockerCommand = GetDockerCliCommand(IsVerbose)
//             .AddArg("pull")
//             .AddArg(fullImageName);
//         PrintCommand(dockerCommand);
//         dockerCommand.Execute();
//         if (dockerCommand.StatusCode != 0
//             || !dockerCommand.StdOutput.Exists(e => 
//                 e.Contains($"Status: Downloaded newer image for {fullImageName}")
//                 || e.Contains($"Status: Image is up to date for {fullImageName}")))
//         {
//             throw new ShellCommandException(1, $"Unable to pull image '{fullImageName}'!");
//         }
//         
//         return dockerCommand.StdOutput
//                    .FirstOrDefault(e => e.StartsWith("Digest: "))
//                    ?.Replace("Digest: ", "")
//                ?? "";
//     }
//
//     protected virtual string GetContainerImageId(string containerName)
//     {
//         var dockerCommand = GetDockerCliCommand(IsVerbose)
//             .AddArg("container")
//             .AddArg("inspect")
//             .AddArg("--format \"{{lower .Image}}\"")
//             .AddArg(containerName);
//         PrintCommand(dockerCommand);
//         dockerCommand.Execute();
//         if (dockerCommand.IsError() || dockerCommand.StdOutput.Count != 1)
//         {
//             throw new ShellCommandException(1, $"Unable to inspect container '{containerName}'!");
//         }
//
//         var result = dockerCommand.StdOutput.First();
//         if (result.StartsWith("sha256:"))
//         {
//             return result;
//         }
//
//         throw new ShellCommandException(1, $"Unable to obtain image ID for container '{containerName}'!");
//     }
//     
//     protected virtual string GetImageId(string image, string? tag = null)
//     {
//         var fullImageName = $"{image}:{tag ?? "latest"}";
//         var dockerCommand = GetDockerCliCommand(IsVerbose)
//             .AddArg("image")
//             .AddArg("inspect")
//             .AddArg("--format \"{{lower .Id}}\"")
//             .AddArg(fullImageName);
//         PrintCommand(dockerCommand);
//         dockerCommand.Execute();
//         if (dockerCommand.IsError() || dockerCommand.StdOutput.Count != 1)
//         {
//             throw new ShellCommandException(1, $"Unable to inspect image '{fullImageName}'!");
//         }
//
//         var result = dockerCommand.StdOutput.First();
//         if (result.StartsWith("sha256:"))
//         {
//             return result;
//         }
//
//         throw new ShellCommandException(1, $"Unable to obtain image ID for '{fullImageName}'!");
//     }
//
//     protected virtual bool CheckIfNewVersionExists(ContainerConfig config)
//     {
//         var imageId = PullImage(config.Image, config.ImageTag);
//         var containerImageId = GetContainerImageId(config.PrimaryName);
//         return containerImageId.Equals(imageId, StringComparison.InvariantCultureIgnoreCase);
//     }
//     
//     protected virtual bool CheckVolume(VolumeConfig volume)
//     {
//         if (volume.IsMapping)
//         {
//             if (Path.Exists(volume.Source))
//             {
//                 return true;
//             }
//
//             if (!volume.AutoCreate)
//             {
//                 return false;
//             }
//
//             PrintCommand("mkdir", volume.Source);
//             Directory.CreateDirectory(volume.Source);
//             if (!ShellCommand.IsWindowsPlatform)
//             {
//                 var bashCommand = GetShellCommand("chgrp", IsVerbose, ShellCommand.BashShell)
//                     .AddArg("docker")
//                     .AddArg(volume.Source);
//                 PrintCommand(bashCommand);
//                 bashCommand.Execute();
//                 if (!bashCommand.IsSuccess(volume.Source))
//                 {
//                     throw new ShellCommandException(1, $"Unable to set group 'docker' for '{volume.Source}' directory!");
//                 }
//             }
//
//             return true;
//         }
//         
//         var dockerCommand = GetDockerCliCommand(IsVerbose)
//             .AddArg("volume")
//             .AddArg("ls")
//             .AddFilterArg(volume.Source);
//         PrintCommand(dockerCommand);
//         dockerCommand.Execute();
//         if (dockerCommand.IsSuccess(volume.Source, true))
//         {
//             return true;
//         }
//
//         if (!volume.AutoCreate)
//         {
//             return false;
//         }
//
//         dockerCommand.ClearArgs()
//             .AddArg("volume")
//             .AddArg("create")
//             .AddArg(volume.Source);
//         PrintCommand(dockerCommand);
//         dockerCommand.Execute();
//         if (!dockerCommand.IsSuccess(volume.Source, true))
//         {
//             throw new ShellCommandException(1, $"Unable to create docker volume '{volume.Source}'!");
//         }
//
//         return true;
//     }
//     
//     protected virtual bool CheckNetwork(NetworkConfig? network)
//     {
//         if (network is null || string.IsNullOrEmpty(network.Name))
//         {
//             return true;
//         }
//         
//         if (string.IsNullOrEmpty(network.Subnet) || string.IsNullOrEmpty(network.IpRange))
//         {
//             return false;
//         }
//         
//         var dockerCommand = GetDockerCliCommand(IsVerbose)
//             .AddArg("network")
//             .AddArg("ls")
//             .AddFilterArg(network.Name);
//         PrintCommand(dockerCommand);
//         dockerCommand.Execute();
//         if (dockerCommand.IsSuccess(network.Name))
//         {
//             return true;
//         }
//
//         dockerCommand.ClearArgs()
//             .AddArg("network")
//             .AddArg("create")
//             .AddArg("-d bridge")
//             .AddArg("--attachable")
//             .AddArg($"--subnet {network.Subnet}")
//             .AddArg($"--ip-range {network.IpRange}")
//             .AddArg(network.Name);
//         PrintCommand(dockerCommand);
//         dockerCommand.Execute();
//         if (dockerCommand.StatusCode != 0 
//             || dockerCommand.StdOutput.Count != 1
//             || string.IsNullOrEmpty(dockerCommand.StdOutput.FirstOrDefault()))
//         {
//             throw new ShellCommandException(1, $"Unable to create docker network '{network.Name}'!");
//         }
//
//         return true;
//     }
//
//     protected virtual bool CheckIfContainerExists(string name)
//     {
//         var dockerCommand = GetDockerCliCommand(IsVerbose)
//             .AddArg("container")
//             .AddArg("ls")
//             .AddArg("--all")
//             .AddFilterArg(name);
//         PrintCommand(dockerCommand);
//         dockerCommand.Execute();
//         return dockerCommand.IsSuccess(name);
//     }
//     
//     
// }