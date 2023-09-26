using Adeotek.DevOpsTools.Common;
using Adeotek.DevOpsTools.Models;

using Spectre.Console;

namespace Adeotek.DevOpsTools.Extensions;

internal static class AnsiConsolePrintExtensions
{
    private const int NameLength = 18;
    private const int SubValueIndent = 2;
    private const char SubValuePrefix = ' ';
    private const string MappingSeparator = " -> ";
    private const string Null = "[null]";
    private const string LabelColor = "gray";
    private const string ValueColor = "aqua";
    private const string SpecialValueColor = "turquoise4";
    private const string SpecialColor = "teal";
    
    internal static void WriteToAnsiConsole(this ContainerConfig config)
    {
        var composer = new CustomComposer()
            .Style(SpecialColor, "[ContainerConfig]").LineBreak()
            .Text("ContainerName:", NameLength).Style(SpecialValueColor, config.PrimaryName).LineBreak()
            .Text("FullImageName:", NameLength).Style(SpecialValueColor, config.FullImageName).LineBreak()
            .Style(LabelColor, "Image:", NameLength).Style(ValueColor, config.Image).LineBreak()
            .Style(LabelColor, "Tag:", NameLength).Style(ValueColor, config.Tag ?? Null).LineBreak()
            .Style(LabelColor, "NamePrefix:", NameLength).Style(ValueColor, config.NamePrefix ?? Null).LineBreak()
            .Style(LabelColor, "BaseName:", NameLength).Style(ValueColor, config.BaseName).LineBreak()
            .Style(LabelColor, "PrimarySuffix:", NameLength).Style(ValueColor, config.PrimarySuffix ?? Null).LineBreak()
            .Style(LabelColor, "BackupSuffix:", NameLength).Style(ValueColor, config.BackupSuffix ?? Null).LineBreak()
            .Style(LabelColor, "Ports:").LineBreak().AddConfigPorts(config.Ports)
            .Style(LabelColor, "Volumes:").LineBreak().AddConfigVolumes(config.Volumes)
            .Style(LabelColor, "EnvVars:").LineBreak().AddConfigEnvVars(config.EnvVars)
            .Style(LabelColor, "Network:").LineBreak()
            .Repeat(SubValuePrefix, SubValueIndent)
            .Style(LabelColor, "Name:", NameLength - SubValueIndent).Style(ValueColor, config.Network?.Name ?? Null).LineBreak()
            .Repeat(SubValuePrefix, SubValueIndent)
            .Style(LabelColor, "Subnet:", NameLength - SubValueIndent).Style(ValueColor, config.Network?.Subnet ?? Null).LineBreak()
            .Repeat(SubValuePrefix, SubValueIndent)
            .Style(LabelColor, "IpRange:", NameLength - SubValueIndent).Style(ValueColor, config.Network?.IpRange ?? Null).LineBreak()
            .Repeat(SubValuePrefix, SubValueIndent)
            .Style(LabelColor, "IpAddress:", NameLength - SubValueIndent).Style(ValueColor, config.Network?.IpAddress ?? Null).LineBreak()
            .Repeat(SubValuePrefix, SubValueIndent)
            .Style(LabelColor, "Hostname:", NameLength - SubValueIndent).Style(ValueColor, config.Network?.Hostname ?? Null).LineBreak()
            .Repeat(SubValuePrefix, SubValueIndent)
            .Style(LabelColor, "Alias:", NameLength - SubValueIndent).Style(ValueColor, config.Network?.Alias ?? Null).LineBreak()
            .Style(LabelColor, "Restart:", NameLength).Style(ValueColor, config.Restart ?? Null).LineBreak();

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
                .Style(SpecialValueColor, port.Host.ToString())
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
                .Style(SpecialValueColor, port.Source)
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
                .Style(SpecialValueColor, key)
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
}