using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Text.Json;

using Adeotek.DevOpsTools.Common;
using Adeotek.DevOpsTools.Extensions;
using Adeotek.DevOpsTools.Models;
using Adeotek.Extensions.Processes;

using Spectre.Console;
using Spectre.Console.Cli;

namespace Adeotek.DevOpsTools.Commands;

internal sealed class DockerContainerCommand : Command<DockerContainerCommand.Settings>
{
    private readonly string _version = Assembly.GetEntryAssembly()
           ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
       ?? "?";
    private int _separatorLength = 80;
    private Settings? _settings;
    private bool IsDebug => _settings?.Debug ?? false;
    
    public sealed class Settings : CommandSettings
    {
        [Description("Config file (with absolute/relative path).")]
        [CommandArgument(0, "<config_file>")]
        public string? ConfigFile { get; init; }
        
        [Description("Force recreation if container exists.")]
        [CommandOption("-f|--force")]
        [DefaultValue(false)]
        public bool Force { get; init; }
        
        [Description("Don't ask for user's input.")]
        [CommandOption("-u|--unattended")]
        [DefaultValue(false)]
        public bool Unattended { get; init; }
        
        [Description("Don't apply any changes, just print the commands.")]
        [CommandOption("--dry-run")]
        [DefaultValue(false)]
        public bool DryRun { get; init; }
        
        [CommandOption("-d|--debug")]
        [DefaultValue(false)]
        public bool Debug { get; init; }
    }

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        try
        {
            PrintStart();
            _settings = settings;
            var config = LoadConfig(settings.ConfigFile);
            if (settings.Debug)
            {
                config.WriteToAnsiConsole();
                PrintSeparator();
            }

            if (CheckIfContainerExists(config.PrimaryName))
            {
                PrintMessage("Container already present!");
                // UpdateContainer(config, settings);
            }
            else
            {
                PrintMessage("Container not fond!");
                if (CreateContainer(config, settings))
                {
                    PrintMessage("Container created successfully!", "green");
                }
                else
                {
                    PrintMessage("Command failed, the container was not created!", "red");
                }
            }

            PrintDone();
            return 0;
        }
        catch (ShellCommandException e)
        {
            if (settings.Debug)
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
            if (settings.Debug)
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

    private bool CreateContainer(ContainerConfig config, Settings settings)
    {
        // CheckNetwork(config);
        
        foreach (var volume in config.Volumes)
        {
            if (!CheckVolume(volume))
            {
                throw new ShellCommandException(1, $"Docker volume '{volume.Source}' is missing!");
            }
        }
        
        return false;
    }
    
    private bool CheckVolume(VolumeConfig volume)
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

            Directory.CreateDirectory(volume.Source);
            if (!ShellCommand.IsWindowsPlatform)
            {
                var bashCommand = GetShellCommand("chgrp", IsDebug, ShellCommand.BashShell)
                    .AddArgument("docker")
                    .AddArgument(volume.Source);
                PrintCommand(bashCommand);
                bashCommand.Execute();
                if (bashCommand.StatusCode == 0 
                    && (bashCommand.StdOutput.FirstOrDefault()?.Contains(volume.Source) ?? false))
                {
                    throw new ShellCommandException(1, $"Unable set directory group to 'docker': {volume.Source}");
                }
            }

            return true;
        }
        
        var shellCommand = GetShellCommand("docker", IsDebug)
            .AddArgument("volume")
            .AddArgument("ls")
            .AddArgument($"--filter name={volume.Source}");
        PrintCommand(shellCommand);
        shellCommand.Execute();
        if (shellCommand.StatusCode == 0 
            && shellCommand.StdOutput.Exists(e => e.Contains(volume.Source)))
        {
            return true;
        }

        if (!volume.AutoCreate)
        {
            return false;
        }

        shellCommand.ClearArguments()
            .AddArgument("volume")
            .AddArgument("create")
            .AddArgument(volume.Source);
        PrintCommand(shellCommand);
        shellCommand.Execute();
        if (shellCommand.StatusCode == 0 
            && (shellCommand.StdOutput.FirstOrDefault()?.Contains(volume.Source) ?? false))
        {
            throw new ShellCommandException(1, $"Unable to create docker volume: {volume.Source}");
        }

        return true;
    }

    private bool CheckIfContainerExists(string name)
    {
        var shellCommand = GetShellCommand("docker", IsDebug)
            .AddArgument("container")
            .AddArgument("ls")
            .AddArgument("--all")
            .AddArgument($"--filter name={name}");
        PrintCommand(shellCommand);
        shellCommand.Execute();
        return shellCommand.StatusCode == 0 
               && shellCommand.StdOutput.Exists(e => e.Contains(name));
    }

    private ContainerConfig LoadConfig(string? configFile)
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
    
    private void PrintStdOutput(object sender, OutputReceivedEventArgs e)
    {
        AnsiConsole.WriteLine(e.Data ?? "._.");
    }

    private void PrintErrOutput(object sender, OutputReceivedEventArgs e)
    {
        AnsiConsole.MarkupLine($"[red]{e.Data ?? "_._"}[/]");
    }

    private void PrintCommand(ShellCommand shellCommand)
    {
        if (!IsDebug)
        {
            return;
        }
        shellCommand.Prepare();
        AnsiConsole.Write(new CustomComposer()
            .Style("aqua", shellCommand.ProcessFile).Space()
            .Style("purple", shellCommand.ProcessArguments).LineBreak());
    }
    
    private void PrintMessage(string message, string? color = null, bool separator = false)
    {
        AnsiConsole.Write(new CustomComposer()
            .Style(color ?? "olive", message).LineBreak());
        if (!separator)
        {
            return;
        }
        PrintSeparator();
    }
    
    private void PrintSeparator(bool big = false)
    {
        AnsiConsole.Write(new CustomComposer()
            .Repeat(big ? '=' : '-', _separatorLength).LineBreak());
    }
    
    private void PrintStart()
    {
        AnsiConsole.Write(new CustomComposer()
            .Text("Running ").Style("purple", "DOT Container Tool").Space()
            .Style("green", $"v{_version}").LineBreak()
            .Repeat('=', _separatorLength).LineBreak());
    }
    
    private void PrintDone()
    {
        AnsiConsole.Write(new CustomComposer()
            .Repeat('=', _separatorLength).LineBreak()
            .Style("purple", "DONE.").LineBreak().LineBreak());
    }

    private ShellCommand GetShellCommand(string command, bool outputRedirect = true, 
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
}