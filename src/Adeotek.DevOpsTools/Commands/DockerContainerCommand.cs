using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;

using Adeotek.CommandLine.Helpers;
using Adeotek.DevOpsTools.Common;
using Adeotek.DevOpsTools.Models;

using Spectre.Console;
using Spectre.Console.Cli;

namespace Adeotek.DevOpsTools.Commands;

internal sealed class DockerContainerCommand : Command<DockerContainerCommand.Settings>
{
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
        [CommandOption("-d|--dry-run")]
        [DefaultValue(false)]
        public bool DryRun { get; init; }
        
        [CommandOption("-v|--verbose")]
        [DefaultValue(false)]
        public bool Verbose { get; init; }
    }
    
    private readonly ShellCommand _shellCommand;
    private readonly bool _supressOutput;

    public DockerContainerCommand()
    {
        _shellCommand = new ShellCommand();
        _shellCommand.OnStdOutput += PrintStdOutput;
        _shellCommand.OnErrOutput += PrintErrOutput;
        _shellCommand.Command = "docker";
    }

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        try
        {
            var config = LoadConfig(settings.ConfigFile);
            var exists = CheckIfContainerExists(config);
            // if (exists)
            // {
            //     CreateContainer(config, settings);
            // }
            // else
            // {
            //     UpdateContainer(config, settings);
            // }

            return 0;
        }
        catch (ShellCommandException e)
        {
            AnsiConsole.WriteException(e, ExceptionFormats.ShortenEverything);
            return e.ExitCode;
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e, ExceptionFormats.ShortenEverything);
            return 1;
        }
    }

    private bool CheckIfContainerExists(ContainerConfig config)
    {
        // _shellCommand.AddArgument("version");
        // _shellCommand.Prepare();
        // PrintCommand();
        // return _shellCommand.Execute();
        return false;
    }

    private ContainerConfig LoadConfig(string? configFile)
    {
        if (string.IsNullOrEmpty(configFile))
        {
            throw new ShellCommandException(2, "The <config_file> command argument is required!");
        }

        if (!File.Exists(configFile))
        {
            throw new ShellCommandException(1, "The <config_file> doesn't exist or is empty!");
        }
        
        var configContent = File.ReadAllText(configFile, Encoding.UTF8);
        if (string.IsNullOrEmpty(configContent))
        {
            throw new ShellCommandException(1, "The <config_file> doesn't exist or is empty!");
        }

        if (Path.GetExtension(configFile) == "json")
        {
            try
            {
                return JsonSerializer.Deserialize<ContainerConfig>(configContent) 
                       ?? throw new ShellCommandException(1, "The <config_file> doesn't exist or is empty!");
            }
            catch (Exception e)
            {
                throw new ShellCommandException(1, "The <config_file> isn't in a valid format!", e);
            }
        }
        
        throw new ShellCommandException(1, "The <config_file> isn't in a valid format!");
    }
    
    private void PrintStdOutput(object sender, OutputReceivedEventArgs e)
    {
        if (_supressOutput)
        {
            return;
        }
        
        AnsiConsole.WriteLine(e.Data ?? "._.");
    }

    private void PrintErrOutput(object sender, OutputReceivedEventArgs e)
    {
        if (_supressOutput)
        {
            return;
        }
        
        AnsiConsole.MarkupLine($"[red]{e.Data ?? "_._"}[/]");
    }

    private void PrintCommand()
    {
        AnsiConsole.MarkupLine($">>> [aqua]{_shellCommand.ProcessFile}[/] [purple]{_shellCommand.ProcessArguments}[/]");
    }
}