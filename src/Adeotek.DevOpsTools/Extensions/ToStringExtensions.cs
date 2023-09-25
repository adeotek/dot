using Adeotek.DevOpsTools.Common;
using Adeotek.DevOpsTools.Models;

using Spectre.Console;

namespace Adeotek.DevOpsTools.Extensions;

internal static class AnsiConsolePrintExtensions
{
    private const int NameLength = 20;
    private const char SubValuePrefix = ' ';
    private const string ValueColor = "aqua";
    private const string KeyColor = "dodgerblue3";
    private const string MappingSeparator = " -> ";
    
    internal static void WriteToAnsiConsole(this ContainerConfig config)
    {
        var composer = new CustomComposer()
            .Style("purple", "[ContainerConfig]").LineBreak()
            .Text("Image:", NameLength).Style(ValueColor, config.Image).LineBreak()
            .Text("ImageTag:", NameLength).Style(ValueColor, config.ImageTag ?? "[Null]").LineBreak()
            .Text("BaseName:", NameLength).Style(ValueColor, config.BaseName).LineBreak()
            .Text("NamePrefix:", NameLength).Style(ValueColor, config.NamePrefix ?? "[Null]").LineBreak()
            .Text("PrimarySuffix:", NameLength).Style(ValueColor, config.PrimarySuffix ?? "[Null]").LineBreak()
            .Text("BackupSuffix:", NameLength).Style(ValueColor, config.BackupSuffix ?? "[Null]").LineBreak()
            .Text("Ports:").LineBreak().AddConfigPorts(config.Ports)
            .Text("Volumes:").LineBreak().AddConfigVolumes(config.Volumes)
            .Text("EnvVars:").LineBreak().AddConfigEnvVars(config.EnvVars)
            .Text("NetworkName:", NameLength).Style(ValueColor, config.NetworkName ?? "[Null]").LineBreak()
            .Text("NetworkSubnet:", NameLength).Style(ValueColor, config.NetworkSubnet ?? "[Null]").LineBreak()
            .Text("NetworkIpRange:", NameLength).Style(ValueColor, config.NetworkIpRange ?? "[Null]").LineBreak()
            .Text("Hostname:", NameLength).Style(ValueColor, config.Hostname ?? "[Null]").LineBreak()
            .Text("NetworkAlias:", NameLength).Style(ValueColor, config.NetworkAlias ?? "[Null]").LineBreak()
            .Text("Restart:", NameLength).Style(ValueColor, config.Restart ?? "[Null]").LineBreak();

        AnsiConsole.Write(composer);
    }

    internal static CustomComposer AddConfigPorts(this CustomComposer composer, PortMapping[] ports)
    {
        if (ports.Length == 0)
        {
            composer.Repeat(SubValuePrefix, NameLength - 1).Space()
                .Style(ValueColor, "[None]").LineBreak();
            return composer;
        }
        
        foreach (var port in ports)
        {
            composer.Repeat(SubValuePrefix, NameLength - 1).Space()
                .Style(ValueColor, port.Host.ToString())
                .Text(MappingSeparator).Style(ValueColor, port.Container.ToString())
                .LineBreak();
        }

        return composer;
    }
    
    internal static CustomComposer AddConfigVolumes(this CustomComposer composer, VolumeConfig[] volumes)
    {
        if (volumes.Length == 0)
        {
            composer.Repeat(SubValuePrefix, NameLength - 1).Space()
                .Style(ValueColor, "[None]").LineBreak();
            return composer;
        }
        
        foreach (var port in volumes)
        {
            composer.Repeat(SubValuePrefix, NameLength - 1).Space()
                .Style(ValueColor, port.Source)
                .Text(MappingSeparator).Style(ValueColor, port.Destination).Text(" (")
                .Style(port.IsMapping ? "green" : "red",$"IsMapping={(port.IsMapping ? "Yes" : "No")}").Text(" | ")
                .Style(port.IsReadonly ? "green" : "red",$"IsReadonly={(port.IsReadonly ? "Yes" : "No")}").Text(" | ")
                .Style(port.AutoCreate ? "green" : "red",$"AutoCreate={(port.AutoCreate ? "Yes" : "No")}")
                .Text(")").LineBreak();
        }

        return composer;
    }
    
    internal static CustomComposer AddConfigEnvVars(this CustomComposer composer, Dictionary<string, string> envVars)
    {
        if (envVars.Count == 0)
        {
            composer.Repeat(SubValuePrefix, NameLength - 1).Space()
                .Style(ValueColor, "[None]").LineBreak();
            return composer;
        }
        
        foreach ((string key, string value) in envVars)
        {
            composer.Repeat(SubValuePrefix, NameLength - 1).Space()
                .Style(KeyColor, key)
                .Text("=").Style(ValueColor, value).LineBreak();
        }

        return composer;
    }

    internal static void WriteToAnsiConsole(this ShellCommandException exception)
    {
        AnsiConsole.MarkupLineInterpolated($"[purple]ERROR({exception.ExitCode}):[/] [red]{exception.Message}[/]");
    }
    
    internal static void WriteToAnsiConsole(this Exception exception)
    {
        AnsiConsole.MarkupLineInterpolated($"[purple]ERROR(1):[/] [red]{exception.Message}[/]");
    }

    private static CustomComposer Text(this CustomComposer composer, string text, int length)
    {
        composer.Text(text);
        if (text.Length < length)
        {
            composer.Repeat(' ', length - text.Length);
        }
        return composer;
    }
}