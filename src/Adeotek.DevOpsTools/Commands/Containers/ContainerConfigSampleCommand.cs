using Adeotek.DevOpsTools.CommandsSettings.Containers;
using Adeotek.Extensions.Docker;

using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Json;

namespace Adeotek.DevOpsTools.Commands.Containers;

internal class ContainerConfigSampleCommand : CommandBase<ContainerConfigSampleSettings>
{
    protected override string CommandName => "containers config";
    
    protected override int ExecuteCommand(CommandContext context, ContainerConfigSampleSettings settings)
    {
        if (settings.Format?.ToLower() != "yaml" && settings.Format?.ToLower() != "json")
        {
            PrintMessage("Invalid format provided in '-f|--format' option!", _errorColor, IsVerbose);    
            return 1;
        }
        
        var config = DockerConfigManager.GetSerializedSampleConfig(settings.Format);
        
        if (settings.Target?.ToLower() == "screen" || settings.Target?.ToLower() == "display")
        {
            if (settings.Format != "json")
            {
                PrintMessage("YAML Sample Config");
                PrintSeparator();
                AnsiConsole.Write(config);
                return 0;
            }

            PrintMessage("JSON Sample Config");
            PrintSeparator();
            AnsiConsole.Write(new JsonText(config));
            AnsiConsole.WriteLine();
            return 0;
        }

        if (string.IsNullOrWhiteSpace(settings.Target))
        {
            PrintMessage("Invalid argument value for 'config_file'!", _errorColor, IsVerbose);    
            return 1;
        }

        try
        {
            var sampleFile = NormalizeTargetFile(settings.Target, settings.Format);
            File.WriteAllText(sampleFile, config);
            PrintMessage("Sample config file generated: ", _successColor, IsVerbose, true);
            PrintMessage(sampleFile, _verboseColor);
            Changes++;
            return 0;
        }
        catch (Exception e)
        {
            PrintMessage($"Sample config file generated: {settings.Target}", _errorColor);
            if (IsVerbose)
            {
                AnsiConsole.WriteException(e, ExceptionFormats.ShortenEverything);
            }
            return 1;
        }
    }

    private static string NormalizeTargetFile(string target, string format)
    {
        var ext = Path.GetExtension(target).ToLower();
        return ext is ".json" or ".yml" or ".yaml" 
            ? target 
            : $"{target}.{(format == "yaml" ? "yml" : "json")}";
    }
}