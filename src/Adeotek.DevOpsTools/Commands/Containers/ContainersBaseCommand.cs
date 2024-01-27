using Adeotek.DevOpsTools.Common;
using Adeotek.DevOpsTools.Extensions;
using Adeotek.DevOpsTools.CommandsSettings.Containers;
using Adeotek.Extensions.ConfigFiles;
using Adeotek.Extensions.Docker;
using Adeotek.Extensions.Docker.Config;
using Adeotek.Extensions.Docker.Exceptions;

using Spectre.Console;
using Spectre.Console.Cli;

namespace Adeotek.DevOpsTools.Commands.Containers;

internal abstract class ContainersBaseCommand<TSettings> 
    : CommandBase<TSettings> where TSettings : ContainersSettings
{
    protected bool IsDryRun => _settings?.DryRun ?? false;
    protected abstract void ExecuteContainerCommand(ContainersConfig config);

    protected override string GetCommandName() => $"containers {_commandName}";

    protected override int ExecuteCommand(CommandContext context, TSettings settings)
    {
        try
        {
            var configVersion = settings.ConfigV1 ?? context.Data?.ToString() == "v1" ? "v1" : null;
            PrintMessage($"Config Version: {configVersion ?? "v2"}", _standardColor);
            if (!settings.ShowConfig)
            {
                PrintSeparator();
            }
            var config = DockerConfigManager.LoadContainersConfig(settings.ConfigFile, configVersion);
            if (settings.ShowConfig)
            {
                config.WriteToAnsiConsole();
            }
            
            ExecuteContainerCommand(config);
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

    protected virtual List<ServiceConfig> GetTargetServices(ContainersConfig config, string? service = null)
    {
        var targetServices = config.Services.ToServicesEnumerable();
        if (!string.IsNullOrEmpty(service))
        {
            targetServices = targetServices.Where(x => x.ServiceName == service);
        }
        
        return targetServices.ToList();
    }

    protected virtual void BackupServiceVolumes(ServiceConfig? service, string? backupLocation, DockerManager dockerManager)
    {
        if (service?.Volumes is null || service.Volumes.Length == 0)
        {
            PrintMessage($"<{service?.ServiceName}> No volumes to backup!", _warningColor, separator: IsVerbose);
            return;
        }
        
        foreach (var volume in service.Volumes)
        {
            var volumeChanges = dockerManager.BackupVolume(volume, backupLocation ?? "", out var backupFile, IsDryRun);
            Changes += volumeChanges;
            if (!IsVerbose)
            {
                continue;
            }
            if (volumeChanges > 0 || IsDryRun)
            {
                PrintMessage($"<{service?.ServiceName}> {volume.Source} volume backup done -> {backupFile}", _standardColor);
            }
            else
            {
                PrintMessage($"<{service?.ServiceName}> {volume.Source} volume backup failed!", _warningColor);
            }
        }
        
        if (IsDryRun)
        {
            PrintMessage($"<{service?.ServiceName}> Volumes backup finished.", _standardColor);
            PrintMessage("Dry run: No changes were made!", _warningColor);
        }
        else
        {
            PrintMessage($"<{service?.ServiceName}> Volumes backup done!", _successColor);
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
