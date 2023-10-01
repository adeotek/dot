using Adeotek.DevOpsTools.CommandsSettings;
using Adeotek.DevOpsTools.Extensions;

using Spectre.Console;
using Spectre.Console.Cli;

namespace Adeotek.DevOpsTools.Commands;

internal abstract class Utf8BomRemoveCommand: Utf8BomBaseCommand<Utf8BomRemoveSettings> 
{
    protected override int ExecuteCommand(CommandContext context, Utf8BomRemoveSettings settings)
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