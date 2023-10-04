using Adeotek.DevOpsTools.CommandsSettings;
using Adeotek.DevOpsTools.Extensions;
using Adeotek.Extensions.Processes;

using Spectre.Console;
using Spectre.Console.Cli;

namespace Adeotek.DevOpsTools.Commands;

internal class Utf8BomAddCommand : Utf8BomBaseCommand<Utf8BomSettings> 
{
    protected override int ExecuteCommand(CommandContext context, Utf8BomSettings settings)
    {
        try
        {
            return ProcessTarget(
                false,
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