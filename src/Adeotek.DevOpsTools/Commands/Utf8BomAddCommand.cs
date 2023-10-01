using Adeotek.DevOpsTools.CommandsSettings;
using Adeotek.DevOpsTools.Extensions;

using Spectre.Console;
using Spectre.Console.Cli;

namespace Adeotek.DevOpsTools.Commands;

internal abstract class Utf8BomAddCommand: Utf8BomBaseCommand<Utf8BomAddSettings> 
{
    protected override int ExecuteCommand(CommandContext context, Utf8BomAddSettings settings)
    {
        try
        {
            
            return 0;
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