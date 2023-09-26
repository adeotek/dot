using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Text.Json;

using Adeotek.DevOpsTools.Common;
using Adeotek.DevOpsTools.Extensions;
using Adeotek.DevOpsTools.Models;
using Adeotek.DevOpsTools.Settings;
using Adeotek.Extensions.Processes;

using Spectre.Console;
using Spectre.Console.Cli;

namespace Adeotek.DevOpsTools.Commands;

internal abstract class ContainerBaseCommand<TSettings> 
    : Command<TSettings> where TSettings : ContainerSettings
{
    protected static readonly string Version = Assembly.GetEntryAssembly()
           ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
       ?? "?";
    protected readonly int _separatorLength = 80;
    protected readonly string _errorColor = "red";
    protected readonly string _warningColor = "olive";
    protected readonly string _standardColor = "turquoise4";
    protected readonly string _successColor = "green";
    protected string? _errOutputColor = "red";
    protected TSettings? _settings;
    protected CommandContext? _context;
    protected bool IsVerbose => _settings?.Verbose ?? false;

    protected abstract void ExecuteCommand(ContainerConfig config);
    
    public override int Execute([NotNull] CommandContext context, [NotNull] TSettings settings)
    {
        try
        {
            PrintStart();
            _context = context;
            _settings = settings;
            var config = LoadConfig(settings.ConfigFile);
            if (settings.Verbose)
            {
                config.WriteToAnsiConsole();
            }

            ExecuteCommand(config);

            PrintDone();
            return 0;
        }
        catch (ShellCommandException e)
        {
            if (settings.Verbose)
            {
                AnsiConsole.WriteException(e, ExceptionFormats.ShortenEverything);
            }
            else
            {
                e.WriteToAnsiConsole();
            }

            return e.ExitCode;
        }
        catch (Exception e)
        {
            if (settings.Verbose)
            {
                AnsiConsole.WriteException(e, ExceptionFormats.ShortenEverything);
            }
            else
            {
                e.WriteToAnsiConsole();
            }

            return 1;
        }
        finally
        {
            _settings = null;
        }
    }

    protected virtual bool CreateContainer(ContainerConfig config)
    {
        if (!CheckNetwork(config.Network))
        {
            throw new ShellCommandException(1, $"Docker network '{config.Network?.Name}' is missing or cannot be created!");
        }
        
        foreach (var volume in config.Volumes)
        {
            if (!CheckVolume(volume))
            {
                throw new ShellCommandException(1, $"Docker volume '{volume.Source}' is missing or cannot be created!");
            }
        }

        var dockerCommand = GetDockerCliCommand(IsVerbose)
            .AddArg("run")
            .AddArg("-d")
            .AddArg($"--name={config.PrimaryName}")
            .AddPortArgs(config.Ports)
            .AddVolumeArgs(config.Volumes)
            .AddEnvVarArgs(config.EnvVars)
            .AddNetworkArgs(config)
            .AddRestartArg(config.Restart)
            .AddArg(config.FullImageName);
        
        PrintCommand(dockerCommand);
        _errOutputColor = null;
        dockerCommand.Execute();
        _errOutputColor = null;
        return dockerCommand.StatusCode == 0
            || dockerCommand.StdOutput.Count == 1
            || string.IsNullOrEmpty(dockerCommand.StdOutput.FirstOrDefault());
    }

    protected virtual bool UpdateContainer(ContainerConfig config)
    {
        return false;
    }

    protected virtual bool RemoveContainer(ContainerConfig config)
    {
        return false;
    }
    
    protected virtual bool CheckVolume(VolumeConfig volume)
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

            PrintCommand("mkdir", volume.Source);
            Directory.CreateDirectory(volume.Source);
            if (!ShellCommand.IsWindowsPlatform)
            {
                var bashCommand = GetShellCommand("chgrp", IsVerbose, ShellCommand.BashShell)
                    .AddArgument("docker")
                    .AddArgument(volume.Source);
                PrintCommand(bashCommand);
                bashCommand.Execute();
                if (!bashCommand.IsSuccess(volume.Source))
                {
                    throw new ShellCommandException(1, $"Unable set group 'docker' for '{volume.Source}' directory!");
                }
            }

            return true;
        }
        
        var dockerCommand = GetDockerCliCommand(IsVerbose)
            .AddArg("volume")
            .AddArg("ls")
            .AddFilterArg(volume.Source);
        PrintCommand(dockerCommand);
        dockerCommand.Execute();
        if (dockerCommand.IsSuccess(volume.Source))
        {
            return true;
        }

        if (!volume.AutoCreate)
        {
            return false;
        }

        dockerCommand.ClearArgs()
            .AddArg("volume")
            .AddArg("create")
            .AddArg(volume.Source);
        PrintCommand(dockerCommand);
        dockerCommand.Execute();
        if (!dockerCommand.IsSuccess(volume.Source, true))
        {
            throw new ShellCommandException(1, $"Unable to create docker volume '{volume.Source}'!");
        }

        return true;
    }
    
    protected virtual bool CheckNetwork(NetworkConfig? network)
    {
        if (network is null || string.IsNullOrEmpty(network.Name))
        {
            return true;
        }
        
        if (string.IsNullOrEmpty(network.Subnet) || string.IsNullOrEmpty(network.IpRange))
        {
            return false;
        }
        
        var dockerCommand = GetDockerCliCommand(IsVerbose)
            .AddArg("network")
            .AddArg("ls")
            .AddFilterArg(network.Name);
        PrintCommand(dockerCommand);
        dockerCommand.Execute();
        if (dockerCommand.IsSuccess(network.Name))
        {
            return true;
        }

        dockerCommand.ClearArgs()
            .AddArg("network")
            .AddArg("create")
            .AddArg("-d bridge")
            .AddArg("--attachable")
            .AddArg($"--subnet {network.Subnet}")
            .AddArg($"--ip-range {network.IpRange}")
            .AddArg(network.Name);
        PrintCommand(dockerCommand);
        dockerCommand.Execute();
        if (dockerCommand.StatusCode != 0 
            || dockerCommand.StdOutput.Count != 1
            || string.IsNullOrEmpty(dockerCommand.StdOutput.FirstOrDefault()))
        {
            throw new ShellCommandException(1, $"Unable to create docker network '{network.Name}'!");
        }

        return true;
    }

    protected virtual bool CheckIfContainerExists(string name)
    {
        var dockerCommand = GetDockerCliCommand(IsVerbose)
            .AddArg("container")
            .AddArg("ls")
            .AddArg("--all")
            .AddFilterArg(name);
        PrintCommand(dockerCommand);
        dockerCommand.Execute();
        return dockerCommand.IsSuccess(name);
    }

    protected virtual ContainerConfig LoadConfig(string? configFile)
    {
        if (string.IsNullOrEmpty(configFile))
        {
            throw new ShellCommandException(2, "The 'config_file' command argument is required!");
        }

        if (!File.Exists(configFile))
        {
            throw new ShellCommandException(1, "The 'config_file' doesn't exist or is empty!");
        }
        
        var configContent = File.ReadAllText(configFile, Encoding.UTF8);
        if (string.IsNullOrEmpty(configContent))
        {
            throw new ShellCommandException(1, "The 'config_file' doesn't exist or is empty!");
        }

        if (Path.GetExtension(configFile) == ".json")
        {
            try
            {
                return JsonSerializer.Deserialize<ContainerConfig>(configContent) 
                       ?? throw new ShellCommandException(1, "The 'config_file' doesn't exist or is empty!");
            }
            catch (Exception e)
            {
                throw new ShellCommandException(1, "The 'config_file' isn't in a valid format!", e);
            }
        }
        
        throw new ShellCommandException(1, "The 'config_file' isn't in a valid format!");
    }

    protected virtual void PrintCommand(ShellCommand shellCommand)
    {
        if (!IsVerbose)
        {
            return;
        }
        shellCommand.Prepare();
        PrintSeparator();
        AnsiConsole.Write(new CustomComposer()
            .Style("aqua", shellCommand.ProcessFile).Space()
            .Style("purple", shellCommand.ProcessArguments).LineBreak());
    }
    
    protected virtual void PrintCommand(string command, string arguments = "")
    {
        if (!IsVerbose)
        {
            return;
        }
        AnsiConsole.Write(new CustomComposer()
            .Style("aqua", command).Space()
            .Style("purple", arguments).LineBreak());
    }
    
    protected virtual void PrintMessage(string message, string? color = null, bool separator = false)
    {
        AnsiConsole.Write(new CustomComposer()
            .Style(color ?? _standardColor, message).LineBreak());
        if (!separator)
        {
            return;
        }
        PrintSeparator();
    }
    
    protected virtual void PrintSeparator(bool big = false)
    {
        AnsiConsole.Write(new CustomComposer()
            .Repeat("gray", big ? '=' : '-', _separatorLength).LineBreak());
    }
    
    protected virtual void PrintStart()
    {
        AnsiConsole.Write(new CustomComposer()
            .Text("Running ").Style("purple", "DOT Container Tool").Space()
            .Style("green", $"v{Version}").LineBreak()
            .Repeat("gray", '=', _separatorLength).LineBreak());
    }
    
    protected virtual void PrintDone()
    {
        AnsiConsole.Write(new CustomComposer()
            .Repeat("gray", '=', _separatorLength).LineBreak()
            .Style("purple", "DONE.").LineBreak().LineBreak());
    }

    protected virtual ShellCommand GetShellCommand(string command, bool outputRedirect = true, 
        string shellName = "")
    {
        var shellCommand = new ShellCommand { ShellName = shellName, Command = command };
        if (outputRedirect)
        {
            shellCommand.OnStdOutput += PrintStdOutput;
            shellCommand.OnErrOutput += PrintErrOutput;    
        }
        return shellCommand;
    }
    
    protected virtual DockerCliCommand GetDockerCliCommand(bool outputRedirect = true)
    {
        var shellCommand = new DockerCliCommand();
        if (outputRedirect)
        {
            shellCommand.OnStdOutput += PrintStdOutput;
            shellCommand.OnErrOutput += PrintErrOutput;    
        }
        return shellCommand;
    }
    
    protected virtual void PrintStdOutput(object sender, OutputReceivedEventArgs e)
    {
        AnsiConsole.WriteLine(e.Data ?? "._.");
    }

    protected virtual void PrintErrOutput(object sender, OutputReceivedEventArgs e)
    {
        if (!string.IsNullOrEmpty(_errOutputColor))
        {
            AnsiConsole.MarkupLine($"[yellow](!)[/] [{_errOutputColor}]{e.Data ?? "_._"}[/]");
            return;
        }
        if (string.IsNullOrEmpty(e.Data))
        {
            return;
        }
        
        AnsiConsole.MarkupLine(e.Data);
    }
}