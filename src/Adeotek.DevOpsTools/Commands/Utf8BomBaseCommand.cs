using Adeotek.DevOpsTools.CommandsSettings;
using Adeotek.DevOpsTools.Extensions;

using Spectre.Console;
using Spectre.Console.Cli;

namespace Adeotek.DevOpsTools.Commands;

internal abstract class Utf8BomBaseCommand<TSettings> 
    : CommandBase<TSettings> where TSettings : Utf8BomSettings
{
    protected override int ExecuteCommand(CommandContext context, TSettings settings)
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