using Adeotek.DevOpsTools.Common;
using Adeotek.DevOpsTools.Extensions;
using Adeotek.DevOpsTools.CommandsSettings.Containers;
using Adeotek.Extensions.ConfigFiles;
using Adeotek.Extensions.Docker;
using Adeotek.Extensions.Docker.Config;
using Adeotek.Extensions.Docker.Config.V1;
using Adeotek.Extensions.Docker.Exceptions;

using Spectre.Console;
using Spectre.Console.Cli;

namespace Adeotek.DevOpsTools.Commands.Containers;

internal abstract class ContainerBaseCommand<TSettings> 
    : CommandBase<TSettings> where TSettings : ContainerSettings
{
    protected bool IsDryRun => _settings?.DryRun ?? false;
    protected abstract void ExecuteContainerCommand(ContainerConfigV1 config);

    protected override int ExecuteCommand(CommandContext context, TSettings settings)
    {
        try
        {
            var configVersion = settings.ConfigV1 ?? context.Data?.ToString() == "v1" ? "v1" : null;
            PrintMessage($"Config Version: {configVersion ?? "v2"}", _standardColor);
            var config = DockerConfigManager.LoadContainersConfig(settings.ConfigFile, configVersion);
            if (settings.ShowConfig)
            {
                config.WriteToAnsiConsole();
            }
            
            // ExecuteContainerCommand(config);
            return 0;
        }
        catch (ConfigFileException e)
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
        catch (DockerCliException e)
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
    }
    
    protected virtual DockerManager GetDockerManager()
    {
        DockerManager dockerManager = new();
        dockerManager.OnDockerCliEvent += HandleDockerCliEvent;
        return dockerManager;
    }

    private void HandleDockerCliEvent(object sender, DockerCliEventArgs e)
    {
        switch (e.Type)
        {
            case DockerCliEventType.Command:
                if (IsSilent || (!IsVerbose && !IsDryRun))
                {
                    break;
                }
                PrintSeparator();
                AnsiConsole.Write(new CustomComposer()
                    .Style("purple", e.Data.GetValueOrDefault("cmd") ?? "?")
                    .Space()
                    .Style("aqua", e.Data.GetValueOrDefault("args") ?? "?").LineBreak());
                break;
            case DockerCliEventType.Message:
                if (IsSilent)
                {
                    break;
                }
                var level = e.Data.GetValueOrDefault("level") ?? "";
                if (IsVerbose && level == "msg")
                {
                    PrintSeparator();
                }
                AnsiConsole.Write(new CustomComposer()
                    .Style(GetColorFromLogLevel(level), e.Data.GetValueOrDefault("message") ?? "")
                    .LineBreak());
                break;
            case DockerCliEventType.StdOutput:
                if (IsSilent || !IsVerbose)
                {
                    break;        
                }
                AnsiConsole.WriteLine(e.DataToString(Environment.NewLine, "._."));
                break;
            case DockerCliEventType.ErrOutput:
                if (!IsVerbose)
                {
                    break;        
                }
                var data = e.DataToString(Environment.NewLine, "_._");
                if (IsSilent)
                {
                    AnsiConsole.Write(data);
                    break;
                }
                if (!string.IsNullOrEmpty(_errOutputColor))
                {
                    AnsiConsole.MarkupLineInterpolated($"[yellow](!)[/] [{_errOutputColor}]{data}[/]");
                    break;
                }
                if (string.IsNullOrEmpty(data))
                {
                    break;
                }
                AnsiConsole.WriteLine(data);
                break;
            case DockerCliEventType.ExitCode:
                break;
        }
    }

    protected virtual string GetColorFromLogLevel(string level) =>
        level switch
        {
            "info" => _standardColor,
            "msg" => _successColor,
            "warn" => _warningColor,
            "err" => _errorColor,
            _ => _standardColor
        };
}
