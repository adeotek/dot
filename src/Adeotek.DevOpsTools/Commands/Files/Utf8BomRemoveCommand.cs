using Adeotek.DevOpsTools.CommandsSettings.Files;
using Adeotek.DevOpsTools.Extensions;
using Adeotek.Extensions.Processes;

using Spectre.Console;
using Spectre.Console.Cli;

namespace Adeotek.DevOpsTools.Commands.Files;

internal class Utf8BomRemoveCommand : Utf8BomBaseCommand<Utf8BomSettings>
{
    protected override string CommandName => "utf8bom remove";
    protected override string ResultLabel => "Changes";
    
    protected override int ExecuteCommand(CommandContext context, Utf8BomSettings settings)
    {
        try
        {
            return ProcessTarget(
                true,
                settings.TargetPath, 
                settings.FileExtensions, 
                settings.IgnoreDirs, 
                settings.DryRun
            );
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
    }
}