using System.Diagnostics.CodeAnalysis;

using Adeotek.CommandLine.Helpers;

using Spectre.Console;
using Spectre.Console.Cli;

namespace Adeotek.DevOpsTools.Commands;

internal sealed class DockerContainerCommand : Command<DockerContainerCommand.Settings>
{
    private readonly ShellCommand _shellCommand;

    public DockerContainerCommand()
    {
        _shellCommand = new ShellCommand();
        _shellCommand.OnStdOutput += PrintStdOutput;
        _shellCommand.OnErrOutput += PrintErrOutput;
        _shellCommand.Command = "docker";
    }

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        _shellCommand.Arguments.Add("version");
        _shellCommand.Prepare();
        AnsiConsole.MarkupLine(
            $"Command: [aqua]{_shellCommand.ProcessFile}[/] [purple]{_shellCommand.ProcessArguments}[/]");
        return _shellCommand.Execute();
    }

    private void PrintStdOutput(object sender, OutputReceivedEventArgs e)
    {
        AnsiConsole.WriteLine(e.Data ?? "._.");
    }

    private void PrintErrOutput(object sender, OutputReceivedEventArgs e)
    {
        AnsiConsole.MarkupLine($"[red]{e.Data ?? "_._"}[/]");
    }

    public sealed class Settings : CommandSettings
    {
        // [Description("Path to search. Defaults to current directory.")]
        // [CommandArgument(0, "[searchPath]")]
        // public string? SearchPath { get; init; }
        //
        // [CommandOption("-p|--pattern")]
        // public string? SearchPattern { get; init; }
        //
        // [CommandOption("--hidden")]
        // [DefaultValue(true)]
        // public bool IncludeHidden { get; init; }
    }
}